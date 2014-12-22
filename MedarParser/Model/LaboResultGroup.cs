using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class LaboResultGroup
    {
        public LaboResultGroup()
        {
            Results = new List<LaboResult>();
        }

        public string Title { get; set; }
        public IList<LaboResult> Results { get; set; }
    }
}
