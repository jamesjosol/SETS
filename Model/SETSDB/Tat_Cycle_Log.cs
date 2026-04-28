using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Tat_Cycle_Log
    {
        public int Id { get; set; }
        public string SectionCode { get; set; }
        public DateTime CycleStart { get; set; }
        public DateTime? CycleEnd { get; set; }
        public decimal? ElapsedMinutes { get; set; }
        public string? Result { get; set; }       // "Within" | "Outside" | "Appealed"
        public string? BatchNo { get; set; }
        public string? AppealedBy { get; set; }
        public DateTime Created { get; set; }
    }
}
