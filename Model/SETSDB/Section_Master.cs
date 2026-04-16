using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Section_Master
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string BranchCode { get; set; }
        public int AutoNo { get; set; }
        public bool Active { get; set; }
        public string Category { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
