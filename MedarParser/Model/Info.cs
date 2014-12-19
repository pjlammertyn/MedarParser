using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Info
    {
        public Info()
        {
            UnknownParts = new List<string>();
        }

        public string ReferenceNumber { get; set; }
        public string Date { get; set; }
        public IList<string> UnknownParts { get; set; }
    }
}
