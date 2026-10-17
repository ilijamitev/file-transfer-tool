namespace FileTransferTool.Core.Exceptions;

public sealed class TransferVerificationException : Exception
{
    public TransferVerificationException(string message) : base(message)
    {
    }
}