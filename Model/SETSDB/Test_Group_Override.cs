using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class Test_Group_Override
    {
        public int Id { get; set; }
        public string TestCode { get; set; }
        public string OverrideGroup { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }
}