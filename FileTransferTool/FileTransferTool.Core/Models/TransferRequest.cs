namespace FileTransferTool.Core.Models;

public sealed record TransferRequest(string SourcePath, string DestinationFolder);