using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Contingency_Batch
    {
        public int Id { get; set; }
        public string BatchNo { get; set; }
        public string EndorsingBranch { get; set; }
        public string EndorsingSection { get; set; }
        public string EndorsedTo { get; set; }
        public string EndorsedBy { get; set; }
        public DateTime EndorsedAt { get; set; }
        public string Status { get; set; } = "Endorsed";
        public bool IsImported { get; set; } = false;
        public string? ImportedBy { get; set; }
        public DateTime? ImportedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}