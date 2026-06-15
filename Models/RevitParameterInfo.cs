using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdityaRevitDataExchange.Models
{
    public class RevitParameterInfo
    {
        public string Name { get; set; }

        public string Group { get; set; }

        public Object Value { get; set; }

        public string StorageType { get; set; }
    }
}
