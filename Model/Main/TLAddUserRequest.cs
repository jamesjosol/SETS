using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class TLAddUserRequest
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int RoleID { get; set; } = 1;
        // IsAdmin intentionally omitted — TL cannot create admins
        // SectionCode intentionally omitted — always read from session
    }

    public class TLUpdateRoleRequest
    {
        public int RoleID { get; set; }
    }
}
