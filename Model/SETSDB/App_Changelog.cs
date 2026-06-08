using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class App_Changelog
    {
        public int Id { get; set; }
        public string Version { get; set; } = string.Empty;   // e.g. "1.4.0"
        public string Title { get; set; } = string.Empty;     // e.g. "Outbound TAT & Bug Fixes"
        public DateTime ReleasedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}