using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Models;
using FileTransferTool.Core.Storage;
using System.Buffers;

namespace FileTransferTool.Core.Transfer;

internal sealed class ChunkTransfer
{
    private readonly SourceFile _source;
    private readonly DestinationPartialFile _destination;
    private readonly byte[][] _chunkHashes;
    private readonly int _maxAttempts;

    public ChunkTransfer(
        SourceFile source, 
        DestinationPartialFile destination, 
        byte[][] chunkHashes, 
        int maxAttempts)
    {
        _source = source;
        _destination = destination;
        _chunkHashes = chunkHashes;
        _maxAttempts = maxAttempts;
    }

    public async ValueTask CopyAsync(Chunk chunk)
    {
        byte[] sourceArray = ArrayPool<byte>.Shared.Rent(chunk.Length);
        byte[] readBackArray = ArrayPool<byte>.Shared.Rent(chunk.Length);
        try
        {
            Memory<byte> sourceBuffer = sourceArray.AsMemory(0, chunk.Length);
            Memory<byte> readBackBuffer = readBackArray.AsMemory(0, chunk.Length);

            byte[] sourceHash = await _source.ReadAsync(chunk, sourceBuffer);
            _chunkHashes[chunk.Index] = sourceHash;

            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                if (await _destination.WriteAndVerifyAsync(chunk, sourceBuffer, readBackBuffer, sourceHash))
                {
                    return;
                }
            }

            throw new TransferVerificationException(
                $"Chunk {chunk.Index + 1} (position {chunk.Offset}) still didn't match its MD5 after {_maxAttempts} attempts.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sourceArray);
            ArrayPool<byte>.Shared.Return(readBackArray);
        }
    }
}