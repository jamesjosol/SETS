using Model.SETSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Main
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public UserSessionData? Data { get; set; }
    }

    public class UserSessionData
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public string BranchCode { get; set; }
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public int RoleID { get; set; }
        public byte Theme { get; set; }
        public string SectionCategory { get; set; }
    }
}
