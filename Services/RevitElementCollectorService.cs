using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using AdityaRevitDataExchange.Models;
using System;
using System.Diagnostics;

namespace AdityaRevitDataExchange.Services
{
    public class RevitElementCollectorService
    {
        private readonly BuiltInCategory[] categories =
        {
            BuiltInCategory.OST_Walls,
            BuiltInCategory.OST_Doors,
            BuiltInCategory.OST_Windows,
            BuiltInCategory.OST_Floors,
            BuiltInCategory.OST_StructuralColumns
        };

        public List<RevitElementInfo> GetElements(Document doc)
        {
            var result = new List<RevitElementInfo>();

            try
            {
            try
            {
                AdityaRevitDataExchange.Services.Logger.Debug("RevitElementCollectorService.GetElements - starting collection");

                foreach (var category in categories)
                {
                    var elements =
                        new FilteredElementCollector(doc)
                            .OfCategory(category)
                            .WhereElementIsNotElementType()
                            .ToElements();

                    result.AddRange(
                        elements
                            .Where(e => e.Category != null)
                            .Select(e => new RevitElementInfo
                            {
                                ElementId = e.Id.Value,
                                UniqueId = e.UniqueId,
                                Name = e.Name,
                                Category = e.Category.Name,
                                Element = e
                            }));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error collecting elements: {ex}");
            }

            Debug.WriteLine($"Collected {result.Count} elements");

            }
            catch (Exception ex)
            {
                AdityaRevitDataExchange.Services.Logger.Error("RevitElementCollectorService.GetElements - failed", ex);
            }

            AdityaRevitDataExchange.Services.Logger.Debug($"RevitElementCollectorService.GetElements - collected {result.Count} elements");

            return result;
        }
    }
}