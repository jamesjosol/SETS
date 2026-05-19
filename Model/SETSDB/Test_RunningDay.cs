using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// The HCLAB test group this test belongs to (e.g. "CH", "HEMA").
        /// Used to scope visibility: lab section TLs only see entries matching
        /// their section's assigned test groups.
        /// </summary>
        public string? TestGroupCode { get; set; }

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