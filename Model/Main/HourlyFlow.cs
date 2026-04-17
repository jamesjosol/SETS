using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class HourlyFlow
    {
        public string Hour { get; set; }      // e.g. "05:00", "07:00"
        public int Count { get; set; }
        public bool IsCurrent { get; set; }   // true if current hour
    }
}