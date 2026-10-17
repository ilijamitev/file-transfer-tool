using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Models;

namespace FileTransferTool.Core.Planning;

public static class TransferPlanner
{
    public static TransferPlan Plan(TransferRequest request, TransferOptions options)
    {
        // 1) Validate source and destination
        string sourcePath = ResolveFullPath(request.SourcePath, "source");
        string destinationFolder = ResolveFullPath(request.DestinationFolder, "destination");

        if (Directory.Exists(sourcePath))
        {
            throw new TransferSetupException("Source is a folder; enter a file path.");
        }

        if (File.Exists(destinationFolder))
        {
            throw new TransferSetupException("Destination is a file; enter a folder.");
        }

        var sourceFileInfo = new FileInfo(sourcePath);
        if (!sourceFileInfo.Exists)
        {
            throw new TransferSetupException($"Source file not found: {sourcePath}");
        }

        if (sourceFileInfo.LinkTarget is not null)
        {
            throw new TransferSetupException($"Source is a link to {sourceFileInfo.LinkTarget}; enter the path of that file instead.");
        }

        string destinationPath = Path.Combine(destinationFolder, sourceFileInfo.Name);
        if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new TransferSetupException("Source and destination are the same file.");
        }

        return new TransferPlan
        {
            SourcePath = sourcePath,
            DestinationFolder = destinationFolder,
            DestinationPath = destinationPath,
            SourceLength = sourceFileInfo.Length,
            ChunkSizeBytes = options.ChunkSizeBytes,
            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            MaxAttemptsPerChunk = options.MaxAttemptsPerChunk,
            DestinationExists = File.Exists(destinationPath),
        };
    }

    private static string ResolveFullPath(string path, string role)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new TransferSetupException($"The {role} path is empty.");
        }

        try
        {
            return Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or IOException)
        {
            // GetFullPath doesn't read or write files, so an IOException here (PathTooLongException)
            // means a bad path, not a failed copy.
            throw new TransferSetupException($"\"{path}\" is not a valid {role} path.", ex);
        }
    }
}