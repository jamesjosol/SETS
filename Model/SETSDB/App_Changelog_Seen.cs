using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    /// <summary>
    /// Records that a user has acknowledged a specific version's changelog.
    /// Unique constraint: (UserID, Version)
    /// </summary>
    public class App_Changelog_Seen
    {
        public int Id { get; set; }
        public string UserID { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime SeenAt { get; set; }
    }
}