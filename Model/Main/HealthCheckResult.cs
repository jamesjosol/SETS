using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class HealthCheckResult
    {
        public bool Online { get; set; }
        public long LatencyMs { get; set; }
        public string? Error { get; set; }
    }
}