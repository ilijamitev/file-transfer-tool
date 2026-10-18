using System.Security.Cryptography;

namespace FileTransferTool.Core.Hashing;

internal static class ChunkHasher
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "Required by the specification")]
    public static byte[] ComputeMd5(ReadOnlySpan<byte> data) => MD5.HashData(data);

    public static bool Matches(ReadOnlySpan<byte> data, ReadOnlySpan<byte> expectedHash)
    {
        return ComputeMd5(data).AsSpan().SequenceEqual(expectedHash);
    }
}