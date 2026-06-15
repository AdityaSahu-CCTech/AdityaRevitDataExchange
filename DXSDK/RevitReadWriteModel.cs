using AdityaRevitDataExchange.Services;
using Autodesk.DataExchange.BaseModels;
using Autodesk.DataExchange.Core;
using Autodesk.DataExchange.Core.Events;
using Autodesk.DataExchange.Core.Models;
using Autodesk.DataExchange.Interface;
using Autodesk.DataExchange.Models;
using Autodesk.DataExchange.UI.Core.Interfaces;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace AdityaRevitDataExchange.DXSDK
{
    public class RevitReadWriteModel : BaseReadWriteExchangeModel
    {
        internal IInteropBridge Bridge { get; set; }

        private readonly List<DataExchange> localStorage = new List<DataExchange>();
        private const int ViewableGenerationDelayMs = 5000;
        public RevitReadWriteModel(IClient client) : base(client)
        {
            AfterCreateExchange += OnAfterCreateExchange;
        }

        /// <summary>
        /// Returns exchanges visible in DXSDK UI.
        /// Currently returns locally cached exchanges only.
        /// </summary>
        public override async Task<List<DataExchange>>GetExchangesAsync(ExchangeSearchFilter exchangeSearchFilter)
        {
            var exchanges =
                await GetValidExchangesAsync(
                    exchangeSearchFilter,
                    localStorage);

            localStorage.Clear();

            if (exchanges != null)
            {
                localStorage.AddRange(exchanges);
            }

            return localStorage.ToList();
        }

        /// <summary>   q
        /// Returns cached exchanges.
        /// </summary>
        public override List<DataExchange>GetCachedExchanges()
        {
            return localStorage?.ToList()
                ?? new List<DataExchange>();
        }

        /// <summary>
        /// Create/Update exchange.
        /// Actual Revit implementation will come later.
        /// </summary>
        //   public override async Task UpdateExchangeAsync(
        //ExchangeItem exchangeItem,
        //CancellationToken cancellationToken = default)
        //   {
        //       var collector =
        //           new RevitElementCollectorService();

        //       var elements =
        //           collector.GetElements(
        //               RevitContext.CurrentDocument);

        //       var builder =
        //           new RevitExchangeBuilderService();

        //       var model =
        //           builder.Build(
        //               Client,
        //               elements);

        //       var identifier =
        //           new DataExchangeIdentifier
        //           {
        //               ExchangeId = exchangeItem.ExchangeID,
        //               CollectionId = exchangeItem.ContainerID,
        //               HubId = exchangeItem.HubId
        //           };

        //       await Client.SyncExchangeDataAsync(
        //           identifier,
        //           model);
        //   }
        [Obsolete]
        public override async Task UpdateExchangeAsync(ExchangeItem exchangeItem,CancellationToken cancellationToken = default)
        {
            try
            {
                AdityaRevitDataExchange.Services.Logger.Info($"RevitReadWriteModel.UpdateExchangeAsync - starting for Exchange {exchangeItem?.ExchangeID}");
                var collector =
                    new RevitElementCollectorService();

                var elements =
                    collector.GetElements(RevitContext.CurrentDocument);

                AdityaRevitDataExchange.Services.Logger.Debug($"RevitReadWriteModel.UpdateExchangeAsync - collected {elements?.Count} elements");

                var builder =
                    new RevitExchangeBuilderService();
                var identifier =
                    new DataExchangeIdentifier
                    {
                        ExchangeId = exchangeItem.ExchangeID,
                        CollectionId = exchangeItem.ContainerID,
                        HubId = exchangeItem.HubId
                    };
                var elementDataModel =
                    await builder.BuildAsync(
                        Client,
                        elements,identifier);

                //AdityaRevitDataExchange.Services.Logger.Debug($"RevitReadWriteModel.UpdateExchangeAsync - built data model elements count: {elementDataModel.Elements?.Count()}");

                //var identifier =
                    //new DataExchangeIdentifier
                    //{
                    //    ExchangeId = exchangeItem.ExchangeID,
                    //    CollectionId = exchangeItem.ContainerID,
                    //    HubId = exchangeItem.HubId
                    //};

                AdityaRevitDataExchange.Services.Logger.Info("RevitReadWriteModel.UpdateExchangeAsync - calling SyncExchangeDataAsync");
                // C#

                // C#
                //var path = @"C:\AdityaCode\AdityaRevitDataExchange\bin\x64\Debug\ForgeParametersCLR.dll";
                //if (!System.IO.File.Exists(path))
                //{
                //    Logger.Error($"Missing file: {path}");
                //}
                //else
                //{
                //    if (!System.Runtime.InteropServices.NativeLibrary.TryLoad(path, out var handle))
                //    {
                //        Logger.Error($"NativeLibrary.TryLoad failed for {path}");
                //    }
                //    else
                //    {
                //        Logger.Info($"NativeLibrary.TryLoad succeeded for {path}");
                //        System.Runtime.InteropServices.NativeLibrary.Free(handle);
                //    }
                //}
                var response = await Client.SyncExchangeDataAsync(identifier, elementDataModel);
                AdityaRevitDataExchange.Services.Logger.Info("RevitReadWriteModel.UpdateExchangeAsync - SyncExchangeDataAsync completed");

                //await Client.GenerateViewableAsync(
                //    exchangeItem.ExchangeID,
                //    exchangeItem.ContainerID);

                //await this.GenerateViewableAsync(exchangeItem);

                //await UpdateExchangeDetailsAsync(exchangeItem);
            }
            catch (Exception ex)
            {
                AdityaRevitDataExchange.Services.Logger.Error("RevitReadWriteModel.UpdateExchangeAsync - failed", ex);
                TaskDialog.Show(
                    "DXSDK Error",
                    ex.ToString());

                throw;
            }
        }


        [Obsolete]
        private async Task GenerateViewableAsync(ExchangeItem exchangeItem)
        {
            await Task.Run(async () =>
            {
                try
                {
                    // Simulate processing time - in real scenario this would be determined by the actual process
                    await Task.Delay(ViewableGenerationDelayMs);
                    await this.Client.GenerateViewableAsync(exchangeItem.ExchangeID, exchangeItem.ContainerID);
                }
                catch (Exception ex)
                {
                    this._sDKOptions?.Logger?.Error(ex);
                    throw;
                }
            });
        }

        private async Task UpdateExchangeDetailsAsync(
    ExchangeItem exchangeItem)
        {
            try
            {
                var identifier =
                    new DataExchangeIdentifier
                    {
                        ExchangeId = exchangeItem.ExchangeID,
                        CollectionId = exchangeItem.ContainerID,
                        HubId = exchangeItem.HubId
                    };

                var response =
                    await Client.GetExchangeDetailsAsync(
                        identifier);

                var details =
                    response.Value;

                if (details == null)
                    return;

                exchangeItem.FileVersion =
                    details.FileVersionUrn;

                exchangeItem.LastModified =
                    details.LastModifiedTime;

                var existing =
                    localStorage.FirstOrDefault(
                        x => x.ExchangeID ==
                             exchangeItem.ExchangeID);

                if (existing != null)
                {
                    existing.FileVersionId =
                        details.FileVersionUrn;

                    existing.Updated =
                        details.LastModifiedTime;
                }

                _sDKOptions.Storage.Add(
                    "LocalExchanges",
                    localStorage);

                _sDKOptions.Storage.Save();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Unload exchanges.
        /// </summary>
        public override Task<IEnumerable<string>>
            UnloadExchangesAsync(List<ExchangeItem> exchanges)
        {
            IEnumerable<string> unloadedIds = exchanges.Select(x => x.ExchangeID);

            return Task.FromResult(unloadedIds);
        }

        /// <summary>
        /// Highlight elements in Revit.
        /// Will implement later.
        /// </summary>
        public override Task<bool>
            SelectElementsAsync(List<string> exchangeIds)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Save exchange locally.
        /// </summary>
        public void AddLocalExchange(DataExchange exchange)
        {
            if (exchange == null)
                return;

            if (localStorage.Any(x => x.ExchangeID == exchange.ExchangeID))
            {
                return;
            }

            localStorage.Add(exchange);
        }

        

        /// <summary>
        /// Load cached exchanges.
        /// </summary>
        public void SetLocalExchanges(List<DataExchange> exchanges)
        {
            if (exchanges == null)
                return;

            localStorage.Clear();

            localStorage.AddRange(exchanges);
        }

        /// <summary>
        /// Return local exchanges.
        /// </summary>
        public List<DataExchange>
            GetLocalExchanges()
        {
            return localStorage?.ToList() ?? new List<DataExchange>();
        }


        private void OnAfterCreateExchange(
    object sender,
    AfterCreateExchangeEventArgs e)
        {
            if (e?.DataExchange == null)
                return;

            AddLocalExchange(e.DataExchange);

            try
            {
                _sDKOptions.Storage.Add(
                    "LocalExchanges",
                    localStorage);

                _sDKOptions.Storage.Save();
            }
            catch
            {
            }
        }
    }
}