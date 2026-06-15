using AdityaRevitDataExchange.Services;
using Autodesk.DataExchange;
using Autodesk.DataExchange.Core.Models;
using Autodesk.DataExchange.Interface;
using Autodesk.DataExchange.UI.Core;
using Autodesk.DataExchange.UI.Core.EventArgs;
using Autodesk.DataExchange.UI.Core.Interfaces;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowStateEnum = Autodesk.DataExchange.UI.Core.Enums.WindowState;

namespace AdityaRevitDataExchange.DXSDK
{
    public class RevitConnectorHost
    {
        private RevitReadWriteModel readWriteModel;
        private SDKOptionsDefaultSetup sdkOptions;
        private IClient client;

        public async Task StartAsync()
        {
            InitializeConnector();
        }

        private void InitializeConnector()
        {
            try
            {
                AdityaRevitDataExchange.Services.Logger.Info("RevitConnectorHost.InitializeConnector - starting");
                string authClientId = ConnectorConfiguration.ClientId;

                string authCallback = ConnectorConfiguration.Callback;

                string connectorName = ConnectorConfiguration.ConnectorName;

                string connectorVersion = ConnectorConfiguration.ConnectorVersion;

                string hostApplicationName = ConnectorConfiguration.HostApplicationName;

                string hostApplicationVersion = ConnectorConfiguration.HostApplicationVersion;

                ValidateConfiguration(
                    authClientId,
                    authCallback,
                    connectorName,
                    connectorVersion,
                    hostApplicationName,
                    hostApplicationVersion);

                sdkOptions =new SDKOptionsDefaultSetup()
                    {
                        ClientId = authClientId,
                        CallBack = authCallback,
                        ConnectorName = connectorName,
                        ConnectorVersion = connectorVersion,
                        HostApplicationName = hostApplicationName,
                        HostApplicationVersion = hostApplicationVersion
                    };

                client = new Client(sdkOptions);

                readWriteModel = new RevitReadWriteModel(client);

                AdityaRevitDataExchange.Services.Logger.Debug("RevitConnectorHost.InitializeConnector - client and readWriteModel created");
                //LoadLocalExchanges();

                var bridgeOptions = InteropBridgeOptions.FromClient(client);

                bridgeOptions.Exchange = readWriteModel;

                bridgeOptions.Invoker =new RevitInteropInvoker(System.Windows.Threading.Dispatcher.CurrentDispatcher);

                bridgeOptions.FeedbackUrl = "https://company-feedback-url";

                var bridge = InteropBridgeFactory.Create(bridgeOptions);

                readWriteModel.Bridge = bridge;

                bridge.ClientStateChanged += OnClientStateChanged;

                _ = InitializeAndLaunchConnectorUi(bridge);
                AdityaRevitDataExchange.Services.Logger.Info("RevitConnectorHost.InitializeConnector - initialized and launched UI");
            }
            catch (Exception ex)
            {
                AdityaRevitDataExchange.Services.Logger.Error("RevitConnectorHost.InitializeConnector - failed", ex);
                TaskDialog.Show(
                    "DXSDK Error",
                    ex.ToString());
            }
        }

        private void ValidateConfiguration(string clientId,string callback,string connectorName,string connectorVersion,string hostName,string hostVersion)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ConfigurationErrorsException("ClientId missing.");

            if (string.IsNullOrWhiteSpace(callback))
                throw new ConfigurationErrorsException("Callback missing.");

            if (!callback.EndsWith("/"))
                throw new ConfigurationErrorsException("Callback must end with '/'");

            if (string.IsNullOrWhiteSpace(connectorName))
                throw new ConfigurationErrorsException("ConnectorName missing.");

            if (string.IsNullOrWhiteSpace(hostName))
                throw new ConfigurationErrorsException("HostApplicationName missing.");
        }

        private async Task InitializeAndLaunchConnectorUi(IInteropBridge interopBridge)
        {
            try
            {
                await interopBridge.InitializeAsync();

                await interopBridge.LaunchConnectorUiAsync();
            }
            catch (Exception ex)
            {
                sdkOptions?.Logger?.Error(ex);

                TaskDialog.Show(
                    "DXSDK Error",
                    ex.Message);
            }
        }

        private void OnClientStateChanged(object sender,ClientStateChangedEventArgs e)
        {
            if (!e.IsConnected)
                return;

            AdityaRevitDataExchange.Services.Logger.Info("RevitConnectorHost.OnClientStateChanged - client connected");

            readWriteModel?.Bridge?
                .SetDocumentName(
                    RevitContext.CurrentDocument?.Title
                    ?? "Revit Document");
        }

        private void LoadLocalExchanges()
        {
            try
            {
                var exchanges =
                    sdkOptions.Storage.Get<List<DataExchange>>(
                        "LocalExchanges");

                if (exchanges != null &&
                    exchanges.Count > 0)
                {
                    readWriteModel.SetLocalExchanges(exchanges);
                }
               

            }
            catch (Exception ex)
            {
                sdkOptions?.Logger?.Error(ex);
            }
        }

        public void Destroy()
        {
            try
            {
                if (readWriteModel?.Bridge != null)
                {
                    readWriteModel.Bridge.SetWindowState(WindowStateEnum.Close);

                    InteropBridgeFactory.DestroyAsync(readWriteModel.Bridge);
                }
            }
            catch (Exception ex)
            {
                sdkOptions?.Logger?.Error(ex);
            }
        }
    }
}
