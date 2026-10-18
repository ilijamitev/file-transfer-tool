namespace FileTransferTool.Core.Models;

public sealed class TransferProgress
{
    private long _bytesCopied;

    public long BytesCopied => Interlocked.Read(ref _bytesCopied);

    internal void AddBytesCopied(int bytes) => Interlocked.Add(ref _bytesCopied, bytes);
}