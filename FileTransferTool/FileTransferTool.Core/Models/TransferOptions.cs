namespace FileTransferTool.Core.Models;

public sealed record TransferOptions
{
    public int ChunkSizeBytes { get; init; } = 1024 * 1024;

    public int MaxDegreeOfParallelism { get; init; } = Math.Min(Environment.ProcessorCount, 8);

    public int MaxAttemptsPerChunk { get; init; } = 3;
}