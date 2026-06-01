using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class AddSectionRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int AutoNo { get; set; }
        public List<string>? TestGroupCodes { get; set; }
    }

    public class UpdateSectionRequest
    {
        public string Name { get; set; } = string.Empty;
        public int AutoNo { get; set; }
        public List<string>? TestGroupCodes { get; set; }
        public string? CutOffTime { get; set; }
        public bool AutoRun { get; set; }
    }
}
