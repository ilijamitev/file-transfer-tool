namespace FileTransferTool.Input;

internal static class ConsoleInput
{
    public static string ReadSourcePath() => ReadPath(@"Enter source file path (e.g. c:\source\my_large_file.bin): ");

    public static string ReadDestinationFolder() => ReadPath(@"Enter destination path (e.g. d:\destination\): ");

    private static string ReadPath(string prompt)
    {
        string? answer;
        do
        {
            Console.Write(prompt);
            answer = Console.ReadLine()?.TrimStart('\uFEFF').Trim().Trim('"');
        }
        while (answer is "");

        return answer ?? "";
    }

    public static bool ConfirmOverwrite(string path)
    {
        Console.Write($"{path} already exists. Overwrite? [y/N] ");
        string? answer = Console.ReadLine()?.Trim();
        return answer is not null && (answer.Equals("y", StringComparison.OrdinalIgnoreCase) || answer.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

}