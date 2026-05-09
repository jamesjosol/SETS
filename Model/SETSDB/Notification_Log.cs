using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Model.SETSDB
{
    public class Notification_Log
    {
        public int NotifID { get; set; }
        public string NotifType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? TargetCategory { get; set; }
        public string? TargetUserID { get; set; }
        public string? TargetSection { get; set; }
        public bool IsRead { get; set; }
        public bool IsAdmin { get; set; }
        public string? ReferenceID { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}