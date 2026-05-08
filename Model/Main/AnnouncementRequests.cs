using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class AnnouncementDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";
        public string TargetRoles { get; set; } = "all";
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAnnouncementRequest
    {
        public string? Title { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info";

        /// <summary>
        /// "all" or comma-separated e.g. "1,2"
        /// </summary>
        public string TargetRoles { get; set; } = "all";

        /// <summary>
        /// datetime | hours | minutes | seconds
        /// </summary>
        public string DurationMode { get; set; } = "datetime";

        public DateTime? ExpiresAt { get; set; }
        public int? DurationValue { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
    }
}