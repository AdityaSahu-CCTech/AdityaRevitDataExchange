using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Reflection;

namespace AdityaRevitDataExchange.Application
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            const string tabName = "Aditya Data Exchange";

            try
            {
                AdityaRevitDataExchange.Services.Logger.Info("App.OnStartup - starting");

                application.CreateRibbonTab(tabName);
            }
            catch
            {
                // Tab already exists
            }

            RibbonPanel panel =
                application.CreateRibbonPanel(
                    tabName,
                    "Data Exchange");

            string assemblyPath =
                Assembly.GetExecutingAssembly().Location;

            PushButtonData buttonData =
                new PushButtonData(
                    "OpenDataExchange",
                    "Data Exchange",
                    assemblyPath,
                    "AdityaRevitDataExchange.Commands.OpenDataExchangeCommand");

            panel.AddItem(buttonData);

            AdityaRevitDataExchange.Services.Logger.Info("App.OnStartup - completed");
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            AdityaRevitDataExchange.Services.Logger.Info("App.OnShutdown");
            return Result.Succeeded;
        }
    }
}
