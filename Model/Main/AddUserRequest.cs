using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class AddUserRequest
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public List<UserSectionEntry> Sections { get; set; } = new();
    }

    public class UpdateUserRequest
    {
        public string? UserName { get; set; }
        public bool IsAdmin { get; set; }
        public bool Active { get; set; }
    }

    public class UpdateUserSectionsRequest
    {
        public List<UserSectionEntry> Sections { get; set; } = new();
    }

    public class UserSectionEntry
    {
        public string SectionCode { get; set; } = string.Empty;
        public int RoleID { get; set; }  // 1 = Regular, 2 = Team Lead, 3 = Admin
    }
}
