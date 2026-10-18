using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Hashing;
using FileTransferTool.Core.Models;
using Microsoft.Win32.SafeHandles;

namespace FileTransferTool.Core.Storage;

internal sealed class DestinationPartialFile : IDisposable
{
    private readonly string _tempPath;
    private SafeFileHandle? _handle;
    private bool _keep;

    private DestinationPartialFile(string tempPath, SafeFileHandle handle)
    {
        _tempPath = tempPath;
        _handle = handle;
    }

    public SafeFileHandle Handle => _handle ?? throw new InvalidOperationException("The partial file is already closed.");

    public static DestinationPartialFile Create(string finalPath, long length)
    {
        string tempPath = $"{finalPath}.{Path.GetRandomFileName()}.part";

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);

            SafeFileHandle handle = File.OpenHandle(tempPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, FileOptions.Asynchronous, preallocationSize: length);
            return new DestinationPartialFile(tempPath, handle);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new TransferSetupException($"Cannot create the destination file: {ex.Message}", ex);
        }
    }

    public async ValueTask<bool> WriteAndVerifyAsync(
        Chunk chunk, 
        ReadOnlyMemory<byte> data, 
        Memory<byte> readBackBuffer, 
        ReadOnlyMemory<byte> expectedHash)
    {
        await RandomAccess.WriteAsync(Handle, data, chunk.Offset);

        int total = 0;
        while (total < chunk.Length)
        {
            int read = await RandomAccess.ReadAsync(Handle, readBackBuffer.Slice(total), chunk.Offset + total);
            if (read == 0)
            {
                throw new IOException($"The temp file ended at byte {chunk.Offset + total}, inside the chunk at {chunk.Offset} that was just written to it.");
            }

            total += read;
        }

        return ChunkHasher.Matches(readBackBuffer.Span, expectedHash.Span);
    }

    public void Flush() => RandomAccess.FlushToDisk(Handle);

    public void Commit(string finalPath, bool overwrite)
    {
        CloseHandle();

        _keep = true;
        try
        {
            File.Move(_tempPath, finalPath, overwrite);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new TransferCommitException($"Renaming the verified copy to {finalPath} failed: {ex.Message} " +
                $"It is kept at {_tempPath}; rename it yourself once the problem is fixed.", ex);
        }
    }

    public void Dispose()
    {
        CloseHandle();
        if (_keep)
        {
            return;
        }

        try
        {
            File.Delete(_tempPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    private void CloseHandle()
    {
        _handle?.Dispose();
        _handle = null;
    }
}