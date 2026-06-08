using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    /// <summary>
    /// A single bullet-point change under a changelog entry.
    /// Tag values: NEW | IMPROVED | FIXED | REMOVED
    /// </summary>
    public class App_Changelog_Item
    {
        public int Id { get; set; }
        public int ChangelogId { get; set; }
        public string Tag { get; set; } = "NEW";          // NEW | IMPROVED | FIXED | REMOVED
        public string Description { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}