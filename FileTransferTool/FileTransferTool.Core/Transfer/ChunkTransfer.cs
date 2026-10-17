using System.Buffers;
using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Models;
using FileTransferTool.Core.Storage;

namespace FileTransferTool.Core.Transfer;

internal sealed class ChunkTransfer
{
    private readonly SourceFile _source;
    private readonly DestinationPartialFile _destination;
    private readonly byte[][] _chunkHashes;
    private readonly int _maxAttempts;

    public ChunkTransfer(SourceFile source, DestinationPartialFile destination, byte[][] chunkHashes, int maxAttempts)
    {
        _source = source;
        _destination = destination;
        _chunkHashes = chunkHashes;
        _maxAttempts = maxAttempts;
    }

    public async ValueTask CopyAsync(Chunk chunk)
    {
        byte[] sendArray = ArrayPool<byte>.Shared.Rent(chunk.Length);
        byte[] receiveArray = ArrayPool<byte>.Shared.Rent(chunk.Length);
        try
        {
            Memory<byte> sendBuffer = sendArray.AsMemory(0, chunk.Length);
            Memory<byte> receiveBuffer = receiveArray.AsMemory(0, chunk.Length);

            byte[] sourceHash = await _source.ReadAsync(chunk, sendBuffer);
            _chunkHashes[chunk.Index] = sourceHash;

            for (int attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                sendBuffer.Span.CopyTo(receiveBuffer.Span);
                if (await _destination.TryReceiveAsync(chunk, receiveBuffer, sourceHash))
                {
                    return;
                }
            }

            throw new TransferVerificationException(
                $"Chunk {chunk.Index + 1} (position {chunk.Offset}) still didn't match its MD5 after {_maxAttempts} attempts.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sendArray);
            ArrayPool<byte>.Shared.Return(receiveArray);
        }
    }
}