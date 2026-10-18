using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Models;

namespace FileTransferTool.Output;

internal static class ConsoleOutput
{
    public static void WriteWelcome()
    {
        WriteInColor("Welcome to File Transfer Tool", ConsoleColor.Cyan);
        Console.WriteLine("It copies a file in chunks, checking every chunk with MD5 and the whole file with SHA-256.");
        Console.WriteLine();
    }

    public static void WriteInColor(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteSuccess(string message) => WriteInColor(message, ConsoleColor.Green);

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteError(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(FormatErrorMessage(ex));
        Console.ResetColor();
    }

    public static void WritePlan(TransferPlan plan)
    {
        Console.WriteLine();
        Console.WriteLine($"Source:      {plan.SourcePath} ({ByteFormatter.Format(plan.SourceLength)})");
        Console.WriteLine($"Destination: {plan.DestinationPath}");
        Console.WriteLine($"Plan:        {plan.ChunkCount:N0} chunks of {ByteFormatter.Format(plan.ChunkSizeBytes)}, {plan.MaxDegreeOfParallelism} in parallel, up to {plan.MaxAttemptsPerChunk} attempts each");
    }

    public static void WriteReport(TransferPlan plan, TransferResult result)
    {
        Console.WriteLine();
        WriteChunkList(plan, result.ChunkHashes);
        Console.WriteLine();
        Console.WriteLine($"Source SHA-256:      {result.SourceFileSha256}");
        Console.WriteLine($"Destination SHA-256: {result.DestinationFileSha256}");
        bool isMatch = string.Equals(result.SourceFileSha256, result.DestinationFileSha256, StringComparison.Ordinal);
        if (isMatch)
        {
            WriteInColor("Result: MATCH", ConsoleColor.Green);
        }
        else
        {
            WriteInColor("Result: MISMATCH", ConsoleColor.Red);
        }
        Console.WriteLine();
        WriteSuccess(FormatSummary(plan, result));
    }

    private static void WriteChunkList(TransferPlan plan, IReadOnlyList<byte[]> chunkHashes)
    {
        Console.WriteLine("Chunk checksums (MD5):");
        if (chunkHashes.Count == 0)
        {
            Console.WriteLine("(no chunks: the file is empty)");
            return;
        }

        for (int index = 0; index < chunkHashes.Count; index++)
        {
            Chunk chunk = plan.ChunkAt(index);
            Console.WriteLine($"{index + 1}) position = {chunk.Offset}, hash = {Convert.ToHexStringLower(chunkHashes[index])}");
        }
    }

    private static string FormatSummary(TransferPlan plan, TransferResult result)
    {
        double seconds = Math.Max(result.Elapsed.TotalSeconds, 0.001);
        string rate = ByteFormatter.Format((long)(plan.SourceLength / seconds));
        return $"Copied and verified {ByteFormatter.Format(plan.SourceLength)} in {result.Elapsed.TotalSeconds:0.00} s ({rate}/s): {plan.ChunkCount:N0} chunks.";
    }

    private static string FormatErrorMessage(Exception ex) => ex switch
    {
        TransferSetupException =>
            $"The transfer did not start. {ex.Message}",

        TransferVerificationException =>
            $"Verification failed: {ex.Message} " +
            "The copy was stopped and its temp file deleted; the destination file was not touched.",

        TransferCommitException =>
            $"The copy is complete and verified, but it could not be given its final name. {ex.Message}",

        IOException or UnauthorizedAccessException =>
            $"File system or permission error: {ex.Message} " +
            "The copy was stopped and its temp file deleted; the destination file was not touched.",

        _ => $"An unexpected error occurred ({ex.GetType().Name}): {ex.Message}",
    };
}