using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Hashing;
using FileTransferTool.Core.Models;
using FileTransferTool.Core.Storage;
using System.Diagnostics;

namespace FileTransferTool.Core.Transfer;

public static class FileTransfer
{
    public static async Task<TransferResult> CopyAsync(TransferPlan plan, TransferProgress progress)
    {
        long startTimestamp = Stopwatch.GetTimestamp();

        // 1) Open the source file and create a temporary destination file
        using SourceFile source = SourceFile.Open(plan.SourcePath);

        long currentSourceLength = RandomAccess.GetLength(source.Handle);
        if (currentSourceLength != plan.SourceLength)
        {
            throw new TransferSetupException($"The source file changed size after it was checked, from {plan.SourceLength} to {currentSourceLength} bytes. Start the copy again.");
        }

        using DestinationPartialFile destination = DestinationPartialFile.Create(plan.DestinationPath, plan.SourceLength);

        // 2) Copy the chunks in parallel
        byte[][] chunkHashes = new byte[plan.ChunkCount][];
        var chunkTransfer = new ChunkTransfer(source, destination, chunkHashes, plan.MaxAttemptsPerChunk, progress);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = plan.MaxDegreeOfParallelism };
        await Parallel.ForAsync(0, plan.ChunkCount, parallelOptions, (index, _) => chunkTransfer.CopyAsync(plan.ChunkAt(index)));

        destination.Flush();

        // 3) Verify the source and destination files' SHA-256 hashes
        Task<string> sourceHashTask = FileHasher.ComputeSha256Async(source.Handle, plan.SourceLength);
        Task<string> destinationHashTask = FileHasher.ComputeSha256Async(destination.Handle, plan.SourceLength);
        await Task.WhenAll(sourceHashTask, destinationHashTask);

        string sourceFileSha256 = await sourceHashTask;
        string destinationFileSha256 = await destinationHashTask;

        if (!string.Equals(sourceFileSha256, destinationFileSha256, StringComparison.Ordinal))
        {
            throw new TransferVerificationException($"The files checksums differ. Source SHA-256: {sourceFileSha256}. Destination SHA-256: {destinationFileSha256}.");
        }

        // 4) Commit the temporary destination file to the final destination path
        destination.Commit(plan.DestinationPath, plan.AllowOverwrite);

        // 5) Return the transfer result
        return new TransferResult
        {
            ChunkHashes = chunkHashes,
            Elapsed = Stopwatch.GetElapsedTime(startTimestamp),
            SourceFileSha256 = sourceFileSha256,
            DestinationFileSha256 = destinationFileSha256
        };
    }
}