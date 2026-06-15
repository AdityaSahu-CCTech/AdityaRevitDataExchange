using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace AdityaRevitDataExchange.Models
{
    public class RevitElementInfo
    {
        public long ElementId { get; set; }

        public string UniqueId { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public Element Element { get; set; }
    }
}