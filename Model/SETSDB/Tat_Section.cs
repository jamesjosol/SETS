using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Tat_Section
    {
        public string SectionCode { get; set; }
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public string AppealWindow { get; set; } 
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}
