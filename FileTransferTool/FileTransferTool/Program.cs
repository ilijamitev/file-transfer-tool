using FileTransferTool;
using FileTransferTool.Core.Exceptions;
using FileTransferTool.Output;


try
{
    await TransferWorkflow.RunAsync();
}
catch (Exception ex)
{
    ConsoleOutput.WriteError(FormatErrorMessage(ex));
}
finally
{
    Console.ReadLine();
}

static string FormatErrorMessage(Exception ex) => ex switch
{
    TransferSetupException =>
        $"The transfer did not start. {ex.Message}",

    TransferVerificationException =>
        $"Verification failed: {ex.Message} " +
        "The copy was stopped and its temp file deleted; the destination file was not touched.",

    TransferCommitException =>
        $"The copy is complete and verified, but it could not be given its final name. {ex.Message}",

    IOException or UnauthorizedAccessException =>
        $"File system or permission error: {ex.Message} " +
        "The copy was stopped and its temp file deleted; the destination file was not touched.",

    _ => $"An unexpected error occurred ({ex.GetType().Name}): {ex.Message}",
};