using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Letter
    {
        public Letter()
        {
            ParserErrors = new Dictionary<int, IList<string>>();
        }

        public Header Header { get; set; }
        public Body Body { get; set; }

        public IDictionary<int, IList<string>> ParserErrors { get; set; }
    }
}
