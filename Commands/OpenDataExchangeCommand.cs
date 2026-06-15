using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AdityaRevitDataExchange.DXSDK;
using AdityaRevitDataExchange.Services;

namespace AdityaRevitDataExchange.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class OpenDataExchangeCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                AdityaRevitDataExchange.Services.Logger.Info("OpenDataExchangeCommand.Execute - starting");

                RevitContext.CurrentDocument =
                    commandData.Application
                        .ActiveUIDocument
                        .Document;

                AdityaRevitDataExchange.Services.Logger.Debug($"Current document set: {RevitContext.CurrentDocument?.Title}");

                var host = new RevitConnectorHost();

                // Wait synchronously and propagate exceptions to caller
                host.StartAsync().GetAwaiter().GetResult();

                AdityaRevitDataExchange.Services.Logger.Info("OpenDataExchangeCommand.Execute - connector host started");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                AdityaRevitDataExchange.Services.Logger.Error("OpenDataExchangeCommand.Execute - failed", ex);
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}