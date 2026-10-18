using FileTransferTool.Core.Exceptions;
using FileTransferTool.Core.Models;
using FileTransferTool.Core.Planning;
using FileTransferTool.Core.Transfer;
using FileTransferTool.Input;
using FileTransferTool.Output;

namespace FileTransferTool;

internal static class TransferWorkflow
{
    public static async Task RunAsync()
    {
        // 1) Get User inputs
        ConsoleOutput.WriteWelcome();

        string sourcePath = ConsoleInput.ReadSourcePath();
        string destinationFolder = ConsoleInput.ReadDestinationFolder();

        // 2) Prepare the Transfer
        var options = new TransferOptions();

        TransferPlan plan = TransferPlanner.Plan(sourcePath, destinationFolder, options);

        if (plan.DestinationExists)
        {
            if (!ConsoleInput.ConfirmOverwrite(plan.DestinationPath))
            {
                throw new TransferSetupException($"Not overwriting {plan.DestinationPath}. Nothing was written.");
            }

            plan = plan with { AllowOverwrite = true };
        }

        ConsoleOutput.WritePlan(plan);

        // 3) Run the transfer
        TransferResult result = await FileTransfer.CopyAsync(plan);

        // 4) Report the result
        ConsoleOutput.WriteReport(plan, result);
    }
}