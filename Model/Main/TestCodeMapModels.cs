using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class TestCodeMapItem
    {
        public int Id { get; set; }
        public string CodeA { get; set; }
        public string CodeB { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? Updated { get; set; }
    }

    public class AddTestCodeMapRequest
    {
        public string CodeA { get; set; }
        public string CodeB { get; set; }
        public string? Remarks { get; set; }
        public string? UserID { get; set; }
    }

    public class UpdateTestCodeMapRequest
    {
        public string CodeA { get; set; }
        public string CodeB { get; set; }
        public string? Remarks { get; set; }
        public string? UserID { get; set; }
    }
}
