namespace FileTransferTool.Core.Models;

public sealed record TransferPlan
{
    public required string SourcePath { get; init; }

    public required string DestinationFolder { get; init; }

    public required string DestinationPath { get; init; }

    public required long SourceLength { get; init; }

    public required int ChunkSizeBytes { get; init; }

    public required int MaxDegreeOfParallelism { get; init; }

    public required int MaxAttemptsPerChunk { get; init; }

    public required bool DestinationExists { get; init; }

    public bool AllowOverwrite { get; init; }

    public int ChunkCount => (int)((SourceLength + ChunkSizeBytes - 1) / ChunkSizeBytes);

}