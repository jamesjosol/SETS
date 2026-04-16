using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.SETSDB
{
    public class User_Master
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public bool Active { get; set; }
        public byte Theme { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public string? UpdatedBy { get; set; }
    }
}