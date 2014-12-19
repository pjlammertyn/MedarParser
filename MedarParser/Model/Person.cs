using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Person
    {
        public Person()
        {
            UnknownParts = new List<string>();
        }

        public string Name { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string PostalName { get; set; }
        public string Phone { get; set; }
        public string RizivNr { get; set; }
        public IList<string> UnknownParts { get; set; }
    }
}
