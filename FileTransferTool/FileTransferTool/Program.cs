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
catch (Exception ex)
{
    ConsoleOutput.WriteError($"An unexpected error occurred ({ex.GetType().Name}): {ex.Message}");
}
finally
{
    Console.ReadLine();
}