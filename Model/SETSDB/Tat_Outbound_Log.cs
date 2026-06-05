using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Tat_Outbound_Log
    {
        public int Id { get; set; }
        public int WindowId { get; set; }
        public DateTime WindowDate { get; set; }
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public string? BatchNos { get; set; }      // comma-separated
        public string Result { get; set; }         // "Within" | "Missed" | "Appealed"
        public string? AppealedBy { get; set; }
        public DateTime? AppealedAt { get; set; }
        public DateTime Created { get; set; }
    }
}