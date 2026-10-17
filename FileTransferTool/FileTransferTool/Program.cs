using FileTransferTool;
using FileTransferTool.Core.Exceptions;
using FileTransferTool.Output;

try
{
    await TransferWorkflow.RunAsync();
}
catch (TransferSetupException ex)
{
    ConsoleOutput.WriteError($"The transfer did not start. {ex.Message}");
}
catch (TransferVerificationException ex)
{
    ConsoleOutput.WriteError($"Verification failed: {ex.Message} The copy was stopped and its temp file deleted; the destination file was not touched.");
}
catch (TransferCommitException ex)
{
    ConsoleOutput.WriteError($"The copy is complete and verified, but it could not be given its final name. {ex.Message}");
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    ConsoleOutput.WriteError($"File system or permission error: {ex.Message} The copy was stopped and its temp file deleted; the destination file was not touched.");
}
catch (Exception ex)
{
    ConsoleOutput.WriteError($"An unexpected error occurred ({ex.GetType().Name}): {ex.Message}");
}
finally
{
    Console.ReadLine();
}