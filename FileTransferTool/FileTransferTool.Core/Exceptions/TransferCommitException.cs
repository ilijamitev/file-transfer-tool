namespace FileTransferTool.Core.Exceptions;

public sealed class TransferCommitException : Exception
{
    public TransferCommitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}