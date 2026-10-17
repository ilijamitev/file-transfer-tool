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

    public async ValueTask<bool> TryReceiveAsync(Chunk chunk, ReadOnlyMemory<byte> received, ReadOnlyMemory<byte> expectedHash)
    {
        if (!ChunkHasher.Matches(received.Span, expectedHash.Span))
        {
            return false;
        }

        await RandomAccess.WriteAsync(Handle, received, chunk.Offset);
        return true;
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
            throw new TransferCommitException(
                $"Renaming the verified copy to {finalPath} failed: {ex.Message} It is kept at {_tempPath}; rename it yourself once the problem is fixed.",
                ex);
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