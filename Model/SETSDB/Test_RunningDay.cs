using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Test_RunningDay
    {
        public int Id { get; set; }
        public string TestCode { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;

        /// <summary>
        /// Semicolon-separated running days, e.g. "Monday;Wednesday;Friday"
        /// </summary>
        public string RunningDays { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }

        // ── Helper (not mapped) ───────────────────────────────────────────────
        public List<string> GetDayList()
            => RunningDays
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .ToList();
    }
}