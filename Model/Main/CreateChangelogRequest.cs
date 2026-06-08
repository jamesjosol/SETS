using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    // ── Request Models ────────────────────────────────────────────────────────

    public class CreateChangelogRequest
    {
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ReleasedAt { get; set; }
        public List<CreateChangelogItemRequest> Items { get; set; } = new();
    }

    public class CreateChangelogItemRequest
    {
        public string Tag { get; set; } = "NEW";      // NEW | IMPROVED | FIXED | REMOVED
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class MarkChangelogSeenRequest
    {
        public string Version { get; set; } = string.Empty;
    }

    // ── Response Models ───────────────────────────────────────────────────────

    public class ChangelogEntryDto
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ReleasedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ChangelogItemDto> Items { get; set; } = new();
    }

    public class ChangelogItemDto
    {
        public int Id { get; set; }
        public string Tag { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
