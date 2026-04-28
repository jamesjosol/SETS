using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class TatUpsertRequest
    {
        public string SectionCode { get; set; } = string.Empty;
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public string AppealWindow { get; set; } = "Before";
    }
}
