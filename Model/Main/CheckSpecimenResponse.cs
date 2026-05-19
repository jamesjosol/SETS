using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class CheckSpecimenResponse
    {
        public string SpecimenNo { get; set; }
        public string BatchNo { get; set; }
        public bool IsFlagged { get; set; }
        public string? FlagReason { get; set; }
        public string? FlaggedBy { get; set; }
        public DateTime? FlaggedAt { get; set; }
    }
}
