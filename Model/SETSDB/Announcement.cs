using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Announcement
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// info | warning | critical
        /// </summary>
        public string Type { get; set; } = "info";

        /// <summary>
        /// "all" or comma-separated category codes e.g. "1,2" or "1,2,3"
        /// </summary>
        public string TargetRoles { get; set; } = "all";

        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? DeactivatedBy { get; set; }
        public DateTime? DeactivatedAt { get; set; }
    }
}