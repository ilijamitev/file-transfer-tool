using System.Diagnostics;
using FileTransferTool.Core.Models;
using FileTransferTool.Core.Storage;

namespace FileTransferTool.Core.Transfer;

public static class FileTransfer
{
    public static async Task<TransferResult> CopyAsync(TransferPlan plan)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        using SourceFile source = SourceFile.Open(plan.SourcePath);
        using DestinationPartialFile destination = DestinationPartialFile.Create(plan.DestinationPath, plan.SourceLength);

        byte[][] chunkHashes = new byte[plan.ChunkCount][];
        var chunkTransfer = new ChunkTransfer(source, destination, chunkHashes, plan.MaxAttemptsPerChunk);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = plan.MaxDegreeOfParallelism };
        await Parallel.ForAsync(0, plan.ChunkCount, parallelOptions, (index, _) => chunkTransfer.CopyAsync(plan.ChunkAt(index)));

        destination.Flush();

        destination.Commit(plan.DestinationPath, plan.AllowOverwrite);

        return new TransferResult
        {
            ChunkHashes = chunkHashes,
            Elapsed = Stopwatch.GetElapsedTime(startTimestamp),
        };
    }
}