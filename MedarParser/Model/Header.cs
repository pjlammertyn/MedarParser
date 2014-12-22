using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Header
    {
        public Person From { get; set; }
        public Person To { get; set; }
        public LetterPatient Subject { get; set; }
        public Info Info { get; set; }
    }
}
