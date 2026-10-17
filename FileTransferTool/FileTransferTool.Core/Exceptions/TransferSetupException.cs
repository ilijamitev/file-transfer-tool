namespace FileTransferTool.Core.Exceptions;

public sealed class TransferSetupException : Exception
{
    public TransferSetupException(string message) : base(message)
    {
    }

    public TransferSetupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}