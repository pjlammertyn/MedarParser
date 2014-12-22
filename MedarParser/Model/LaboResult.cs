using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class LaboResult
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Boundaries { get; set; }
        public string Units { get; set; }
        public string Result { get; set; }
        public string Comment { get; set; }
    }
}
