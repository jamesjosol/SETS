using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class BatchSectionSummary
    {
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public string SectionCategory { get; set; }
        public int TotalEndorsed { get; set; }
        public int Pending { get; set; }
        public int Received { get; set; }
        public int OutsideTAT { get; set; }
    }
}