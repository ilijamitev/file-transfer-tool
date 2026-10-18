using Microsoft.Win32.SafeHandles;
using System.Security.Cryptography;

namespace FileTransferTool.Core.Hashing;

internal static class FileHasher
{
    private const int BlockSizeBytes = 1024 * 1024;

    public static async Task<string> ComputeSha256Async(SafeFileHandle handle, long length)
    {
        using var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        byte[] buffer = new byte[BlockSizeBytes];

        long offset = 0;
        while (offset < length)
        {
            int toRead = (int)Math.Min(BlockSizeBytes, length - offset);
            int read = await RandomAccess.ReadAsync(handle, buffer.AsMemory(0, toRead), offset);
            if (read == 0)
            {
                throw new IOException($"The file ended at byte {offset}, before its expected length of {length} bytes.");
            }

            sha256.AppendData(buffer, 0, read);
            offset += read;
        }

        return Convert.ToHexStringLower(sha256.GetHashAndReset());
    }
}