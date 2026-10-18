using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Models;

namespace FileTransferTool.Core.Planning;

public static class TransferPlanner
{
    public static TransferPlan Plan(string sourcePath, string destinationFolder, TransferOptions options)
    {
        string fullSourcePath = ResolveFullPath(sourcePath, "source");
        string fullDestinationFolder = ResolveFullPath(destinationFolder, "destination");

        if (Directory.Exists(fullSourcePath))
        {
            throw new TransferSetupException("Source is a folder; enter a file path.");
        }

        if (File.Exists(fullDestinationFolder))
        {
            throw new TransferSetupException("Destination is a file; enter a folder.");
        }

        var sourceFileInfo = new FileInfo(fullSourcePath);
        if (!sourceFileInfo.Exists)
        {
            throw new TransferSetupException($"Source file not found: {fullSourcePath}");
        }

        if (sourceFileInfo.LinkTarget is not null)
        {
            throw new TransferSetupException($"Source is a link to {sourceFileInfo.LinkTarget}; enter the path of that file instead.");
        }

        string destinationPath = Path.Combine(fullDestinationFolder, sourceFileInfo.Name);
        if (string.Equals(fullSourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new TransferSetupException("Source and destination are the same file.");
        }

        return new TransferPlan
        {
            SourcePath = fullSourcePath,
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
            throw new TransferSetupException($"\"{path}\" is not a valid {role} path.", ex);
        }
    }

}