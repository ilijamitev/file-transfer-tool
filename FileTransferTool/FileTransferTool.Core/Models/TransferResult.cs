namespace FileTransferTool.Core.Models;

public sealed record TransferResult
{
    public required IReadOnlyList<byte[]> ChunkHashes { get; init; }

    public required TimeSpan Elapsed { get; init; }

    public required string SourceFileSha256 { get; init; }

    public required string DestinationFileSha256 { get; init; }

}