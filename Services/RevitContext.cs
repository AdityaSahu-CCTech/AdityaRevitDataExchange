using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace AdityaRevitDataExchange.Services
{
    public static class RevitContext
    {
        public static Document CurrentDocument { get; set; }
    }
}
