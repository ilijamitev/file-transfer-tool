using FileTransferTool;
using FileTransferTool.Output;

try
{
    await TransferWorkflow.RunAsync();
}
catch (Exception ex)
{
    ConsoleOutput.WriteError(ex);
}

Console.ReadLine();