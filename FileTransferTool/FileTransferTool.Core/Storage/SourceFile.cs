using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Hashing;
using FileTransferTool.Core.Models;
using Microsoft.Win32.SafeHandles;

namespace FileTransferTool.Core.Storage;

internal sealed class SourceFile : IDisposable
{
    private SourceFile(SafeFileHandle handle)
    {
        Handle = handle;
    }

    public SafeFileHandle Handle { get; }

    public static SourceFile Open(string path)
    {
        try
        {
            SafeFileHandle handle = File.OpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous | FileOptions.SequentialScan);
            return new SourceFile(handle);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new TransferSetupException($"Cannot open the source file: {ex.Message}", ex);
        }
    }

    public async ValueTask<byte[]> ReadAsync(Chunk chunk, Memory<byte> buffer)
    {
        int total = 0;
        while (total < chunk.Length)
        {
            int read = await RandomAccess.ReadAsync(Handle, buffer[total..], chunk.Offset + total);
            if (read == 0)
            {
                throw new IOException($"The source file ended at byte {chunk.Offset + total}, before the chunk at {chunk.Offset} was complete. It was shortened during the copy.");
            }

            total += read;
        }

        return ChunkHasher.ComputeMd5(buffer.Span);
    }

    public void Dispose() => Handle.Dispose();
}