using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using AdityaRevitDataExchange.Models;

namespace AdityaRevitDataExchange.Services
{
    public class RevitParameterService
    {
        public List<RevitParameterInfo> GetParameters(Element element)
        {
            var parameters =
                new List<RevitParameterInfo>();

            try
            {
                foreach (Parameter parameter in element.Parameters)
                {
                    try
                    {
                        string name =
                            parameter.Definition?.Name;

                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        Object value =
                            parameter.AsValueString();

                        //if (string.IsNullOrWhiteSpace(value))
                        //{
                        //    value =
                        //        parameter.AsString();
                        //}

                        //if (string.IsNullOrWhiteSpace(value))
                        //    continue;

                        string group =
                            GetGroup(name);

                        parameters.Add(
                            new RevitParameterInfo
                            {
                                Name = name,
                                Group = group,
                                Value = value,
                                StorageType =
                                    parameter.StorageType.ToString()
                            });
                    }
                    catch (Exception ex)
                    {
                        AdityaRevitDataExchange.Services.Logger.Error($"RevitParameterService.GetParameters - param read failed", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                AdityaRevitDataExchange.Services.Logger.Error("RevitParameterService.GetParameters - failed", ex);
            }

            AdityaRevitDataExchange.Services.Logger.Debug($"RevitParameterService.GetParameters - returning {parameters.Count} parameters");

            return parameters;
        }

        private string GetGroup(string parameterName)
        {
            switch (parameterName)
            {
                case "Length":
                case "Area":
                case "Volume":
                case "Width":
                case "Height":
                    return "Dimensions";

                case "Family":
                case "Family Name":
                case "Type":
                case "Type Name":
                case "Description":
                    return "Identity Data";

                case "Level":
                case "Base Constraint":
                case "Top Constraint":
                case "Offset":
                    return "Constraints";

                default:
                    return "Other";
            }
        }
    }
}
