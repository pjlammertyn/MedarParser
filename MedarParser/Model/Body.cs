using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Body
    {
        public Body()
        {
            Exams = new List<Exam>();
        }

        public string Title { get; set; }
        public DateTime? Date { get; set; }
        public IList<Exam> Exams { get; set; }
        public string Text { get; set; }
    }
}
