﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedarParser.Model
{
    public class Labo
    {
        public Labo()
        {
            ResultGroups = new List<LaboResultGroup>();
        }

        public string Code { get; set; }
        public LaboPatient Patient { get; set; }
        public Mutuality Mutuality { get; set; }
        public Contact Contact { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public Doctor Doctor { get; set; }
        public IList<LaboResultGroup> ResultGroups { get; set; }
    }
}
