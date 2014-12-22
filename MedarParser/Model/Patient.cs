using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public abstract class Patient
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string PostalName { get; set; }
        public DateTime? BirthDate { get; set; }
        public Sex Sex { get; set; }
    }

    public class LetterPatient : Patient
    {
        public LetterPatient()
        {
            UnknownParts = new List<string>();
        }

        public string PostalCode { get; set; }
        public IList<string> UnknownParts { get; set; }
    }

    public class LaboPatient : Patient
    {
        public string Key { get; set; }
        public string UniquePatientNumber { get; set; }
        public string UniqueMedicalRecordNumber { get; set; }
    }
}
