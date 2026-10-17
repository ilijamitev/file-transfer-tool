namespace FileTransferTool.Output;

internal static class ByteFormatter
{
    private static readonly string[] _units = ["B", "KiB", "MiB", "GiB", "TiB"];

    public static string Format(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        double value = bytes;
        int unit = 0;
        while (value >= 1024 && unit < _units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.00} {_units[unit]}";
    }
}