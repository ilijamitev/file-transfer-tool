namespace FileTransferTool.Core.Models;

public sealed record TransferResult
{
    public required IReadOnlyList<byte[]> ChunkHashes { get; init; }

    public required TimeSpan Elapsed { get; init; }
}