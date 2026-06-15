using AdityaRevitDataExchange.Models;
using Autodesk.DataExchange;
using Autodesk.DataExchange.Core;
using Autodesk.DataExchange.Core.Models;
using Autodesk.DataExchange.DataModels;
using Autodesk.DataExchange.Interface;
using Autodesk.Parameters;
using Autodesk.Revit.DB;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Autodesk.DataExchange.DataModels.ElementDataModel;
using DXElement = Autodesk.DataExchange.DataModels.Element;
using Parameter = Autodesk.DataExchange.DataModels.Parameter;
using RevitElement = Autodesk.Revit.DB.Element;
namespace AdityaRevitDataExchange.Services
{
    public class RevitExchangeBuilderService
    {
        public async Task<ElementDataModel> BuildAsync(
            IClient client,
            List<RevitElementInfo> revitElements,DataExchangeIdentifier identifier
            )
        {
            var dataModel =
                ElementDataModel.Create(client);

            AdityaRevitDataExchange.Services.Logger.Info("RevitExchangeBuilderService.Build - starting build");

            var geometryService =
                new RevitGeometryService();

            var parameterService =
                new RevitParameterService();

            foreach (var revitElement in revitElements)
            {
                try
                {
                    AdityaRevitDataExchange.Services.Logger.Debug($"Processing element {revitElement.ElementId} - {revitElement.Name}");
                    // C#

                    string elementId =
                        !string.IsNullOrWhiteSpace(revitElement.UniqueId)
                            ? revitElement.UniqueId
                            : Guid.NewGuid().ToString();

                    string name =
                        !string.IsNullOrWhiteSpace(revitElement.Name)
                            ? revitElement.Name
                            : $"Element_{revitElement.ElementId}";

                    string category =
                        !string.IsNullOrWhiteSpace(revitElement.Category)
                            ? revitElement.Category
                            : "Unknown";

                    string family =
                        GetFamilyName(revitElement.Element);

                    string type =
                        GetTypeName(revitElement.Element);

                    var element =
                        dataModel.AddElement(
                            new ElementProperties(elementId, name, category, family, type));

                    AdityaRevitDataExchange.Services.Logger.Debug($"Added DX element {elementId} to ElementDataModel");

                    // keep track of parameters we've already added to this DX element
                    var addedParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    if (revitElement.Element != null)

                    {


                        //Parameter parameter = null;
                        //parameter = new Parameter("TestParameter", new ParameterDataType(10))
                        //{
                        //    IsCustomParameter = true,
                        //    GroupID = Autodesk.Parameters.Group.General.DisplayName()
                        //};
                        //await element.CreateInstanceParameterAsync(parameter);
                        var parameters = parameterService.GetParameters(revitElement.Element);

                        if (parameters != null && parameters.Count > 0)
                        {
                            AdityaRevitDataExchange.Services.Logger.Debug($"Attaching {parameters.Count} parameters for element {elementId}");

                            foreach (var revitParam in parameters)
                            {
                                AdityaRevitDataExchange.Services.Logger.Debug($"Scheduling attach parameter '{revitParam.Name}'='{revitParam.Value}' to element {elementId}");

                                //AddParameterToElementAsync(
                                //    element,
                                //    revitParam.Name,
                                //    revitParam.Value,
                                //    addedParameterNames,
                                //    elementId);

                                IParameter parameter = null;
                                parameter=CreateSdkParameter(revitParam.Name, revitParam.Value, revitParam.Group, addedParameterNames);
                                var nameKey = (revitParam.Name ?? string.Empty).Trim();
                                if (parameter != null) { 
                                    await element.CreateInstanceParameterAsync(parameter); }
                                
                                addedParameterNames?.Add(nameKey);

                                AdityaRevitDataExchange.Services.Logger.Debug($"Scheduled {parameters.Count} parameter attach tasks for element {elementId}");
                            }
                        }




                        //----------------------------------
                        // Geometry
                        //----------------------------------

                        if (revitElement.Element != null)
                        {
                            var geometry =
                                geometryService.CreateGeometry(
                                    revitElement.Element);

                            if (geometry != null)
                            {
                                AdityaRevitDataExchange.Services.Logger.Debug($"Attaching geometry for element {elementId}");
                                dataModel.SetElementGeometry(
                                    element,
                                    new List<ElementGeometry>
                                    {
                                    geometry
                                    });
                            }
                        }
                    }
                }

                // parameters already attached above
                catch (Exception ex)
                {
                    AdityaRevitDataExchange.Services.Logger.Error($"Error processing element {revitElement?.ElementId}", ex);
                }
            }

            

            //await client.SyncExchangeDataAsync(
            //        identifier,
            //        dataModel);
            return dataModel;
        }

        private static IParameter CreateSdkParameter(string name, object value, string group, HashSet<string> addedParameterNames)
        {
            IParameter parameter=null;
            var nameKey = (name ?? string.Empty).Trim();
            if (addedParameterNames != null && addedParameterNames.Contains(nameKey))
                if (value is double doubleValue)
            {
                parameter = new Parameter(name, doubleValue);
            }
            else if (value is int intValue)
            {
                parameter = new Parameter(name, intValue);
            }
            else
            {
                parameter = new Parameter(name, value?.ToString() ?? "");
            }

            parameter.SampleText = name;
            parameter.Description = $"{name} ({group})";
            parameter.ReadOnly = false;

            return parameter;
        }

        private static DXElement AddParameterToElementAsync(
            DXElement element,
            string parameterName,
            string parameterValue,
            HashSet<string> addedParameterNames,
            string elementId)
        {
            var nameKey = (parameterName ?? string.Empty).Trim();

            // Skip if we already added this parameter for this element
            //if (addedParameterNames != null && addedParameterNames.Contains(nameKey))
            //    return;

            IParameter parameter = null;
             parameter = new Parameter("nameKey", new ParameterDataType (10))
            {
                ReadOnly = false,
                IsCustomParameter = true,
                Description = $"Revit Parameter: {nameKey}",
                GroupID =Autodesk.Parameters.Group.General.DisplayName()
             };

            try
            {
                AdityaRevitDataExchange.Services.Logger.Debug($"Creating parameter '{nameKey}' with value '{parameterValue}' for element {elementId}");
                Task.Run(async()=> await element.CreateInstanceParameterAsync(parameter)).Wait();

                addedParameterNames?.Add(nameKey);
                AdityaRevitDataExchange.Services.Logger.Debug($"Parameter '{nameKey}' attached to element {elementId}");
            }
            catch (ArgumentException ex) when (ex.Message?.IndexOf("already exists", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                AdityaRevitDataExchange.Services.Logger.Debug($"Parameter '{nameKey}' already exists on element {elementId}, skipping: {ex.Message}");
                // Duplicate created concurrently or pre-existing on element; ignore or log.
                // Optionally, if SDK supports updating the existing parameter, do that here.
            }
            return element;
        }

        private string GetFamilyName(RevitElement element)
        {
            if (element is FamilyInstance familyInstance)
            {
                return familyInstance.Symbol?.FamilyName
                       ?? "Unknown Family";
            }

            ElementType type =
                element.Document.GetElement(
                    element.GetTypeId())
                as ElementType;

            return type?.FamilyName
                   ?? element.Category?.Name
                   ?? "Unknown Family";
        }

        private string GetTypeName(RevitElement element)
        {
            if (element is FamilyInstance familyInstance)
            {
                return familyInstance.Symbol?.Name
                       ?? "Unknown Type";
            }

            ElementType type =
                element.Document.GetElement(
                    element.GetTypeId())
                as ElementType;

            return type?.Name
                   ?? "Unknown Type";
        }
        // ...

        //string elementId =
        //    !string.IsNullOrWhiteSpace(revitElement.UniqueId)
        //        ? revitElement.UniqueId
        //        : Guid.NewGuid().ToString();

        //string name =
        //    !string.IsNullOrWhiteSpace(revitElement.Name)
        //        ? revitElement.Name
        //        : $"Element_{revitElement.ElementId}";

        //string category =
        //    !string.IsNullOrWhiteSpace(revitElement.Category)
        //        ? revitElement.Category
        //        : "Unknown";

        //string family =
        //    GetFamilyName(revitElement.Element);

        //string type =
        //    GetTypeName(revitElement.Element);

        //var element =
        //    dataModel.AddElement(
        //        new ElementProperties(
        //            elementId,
        //            name,
        //            category,
        //            family,
        //            type));

        //// Collect parameters and attach them synchronously to the DX element
        //if (revitElement.Element != null)
        //{
        //    var parameters = parameterService.GetParameters(revitElement.Element);

        //    if (parameters != null && parameters.Count > 0)
        //    {
        //        AdityaRevitDataExchange.Services.Logger.Debug($"Attaching {parameters.Count} parameters for element {elementId}");
        //        //foreach (var revitParam in parameters)
        //        //{
        //        //    await AddParameterToElementAsync(
        //        //        element,
        //        //        revitParam.Name,
        //        //        revitParam.Value);
        //        //}

        //        // C#


        //            // ...
        //        }
        //    }
        //private async Task AddParameterToElementAsync(DXElement element, string name, object value)
        //{
        //    throw new NotImplementedException();
        //}

        //private static async Task AddParameterToElementAsync(
        //        DXElement dxElement,
        //    string parameterName,
        //    string parameterValue)
        //{
        //    var parameter = new Parameter(
        //        parameterName,
        //        parameterValue)
        //    {
        //        ReadOnly = false,
        //        IsCustomParameter = true,
        //        Description = $"Revit Parameter: {parameterName}",
        //        GroupID = "Data"            
        //    };

        //    await dxElement.CreateInstanceParameterAsync(parameter);
        //}
        // C#
        // C#


        //        
        private void TryAttachParametersToElement(
            ElementDataModel dataModel,
            IClient client,
            DXElement element,
            List<RevitParameterInfo> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return;
            try
            {
                //var parameter = new Autodesk.DataExchange.DataModels.Parameter("df", new ParameterDataType(10));
                //parameter.SampleText = "Sample string parameter";
                //parameter.Description = "Demo string parameter for sample connector";
                //parameter.ReadOnly = false;
                //parameter.IsCustomParameter = true;
                //parameter.GroupID = "General";

                //{
                //    SampleText = "Sample string parameter",
                //    Description = "Demo string parameter for sample connector",
                //    ReadOnly = false,
                //    IsCustomParameter = true,
                //    GroupID = Group.Graphics.DisplayName()
                //};

                //element.CreateInstanceParameterAsync(parameter);
               // var edmType = dataModel.GetType();

               // // 1) Try direct API on ElementDataModel: SetElementParameters
               // var setParamsMethod = edmType.GetMethod("SetElementParameters");

               // if (setParamsMethod != null)
               // {
               //     try
               //     {
               //         var sdkParams = parameters.Select(p => CreateSdkParameterObject(p)).ToArray();
               //         setParamsMethod.Invoke(dataModel, new object[] { element, sdkParams });
               //         return;
               //     }
               //     catch
               //     {
               //         // swallow and try next
               //     }
               // }

               // // 2) Try client synchronous CreateInstanceParameter
               // var clientType = client.GetType();

               // var createSync = clientType.GetMethod("CreateInstanceParameter");

               // if (createSync != null)
               // {
               //     foreach (var p in parameters)
               //     {
               //         try
               //         {
               //             var sdkParam = CreateSdkParameterObject(p);
               //             createSync.Invoke(client, new object[] { element, sdkParam });
               //         }
               //         catch
               //         {
               //         }
               //     }

               //     return;
               // }
               // // 3) Try async create and block until complete
               // var createAsync = clientType.GetMethod("CreateInstanceParameterAsync");
               //// var createAsync = clientType.GetMethod("CreateInstanceParameterAsync");

               // if (createAsync != null)
               // {
               //     foreach (var p in parameters)
               //     {
               //         try
               //         {
               //             var sdkParam = CreateSdkParameterObject(p);
               //             var task = (System.Threading.Tasks.Task)createAsync.Invoke(client, new object[] { element, sdkParam });
               //             task?.GetAwaiter().GetResult();
               //         }
               //         catch
               //         {
               //         }
               //     }

               //     return;
               // }

               // // 4) Fallback: set element metadata
               // var setMeta = edmType.GetMethod("SetElementMetadata") ?? edmType.GetMethod("SetElementProperty");

               // if (setMeta != null)
               // {
               //     try
               //     {
               //         var json = SerializeParameters(parameters);
               //         setMeta.Invoke(dataModel, new object[] { element, "RevitParameters", json });
               //     }
               //     catch
               //     {
               //     }
               // }
            }
            catch (Exception ex)
            {
                AdityaRevitDataExchange.Services.Logger.Error("TryAttachParametersToElement - failed to attach parameters", ex);

            }
        }
        // Note: GetSdkParameterType helper removed. SDK parameter type lookup is now performed inline

        private static object CreateSdkParameterObject(RevitParameterInfo p)
        {
            // best-effort parameter object; if SDK has concrete type, replace with that
            return new Dictionary<string, object>
            {
                ["Name"] = p.Name,
                ["Value"] = p.Value?.ToString(),
                ["StorageType"] = p.StorageType
            } as object;
        }

        private static string SerializeParameters(List<RevitParameterInfo> list)
        {
            if (list == null) return "[]";

            string Escape(string s)
            {
                if (s == null) return "";
                return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
            }

            var parts = new List<string>();

            foreach (var p in list)
            {
                var name = Escape(p.Name);
                var value = Escape(p.Value?.ToString());
                var storage = Escape(p.StorageType);

                parts.Add("{\"Name\":\"" + name + "\",\"Value\":\"" + value + "\",\"StorageType\":\"" + storage + "\"}");
            }

            return "[" + string.Join(",", parts) + "]";
        }

        
    }
}