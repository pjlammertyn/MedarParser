using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Patient
    {
        public Patient()
        {
            UnknownParts = new List<string>();
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string PostalName { get; set; }
        public DateTime? BirthDate { get; set; }
        public Sex Sex { get; set; }
        public IList<string> UnknownParts { get; set; }
    }
}
