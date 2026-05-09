using Model.Main;
using Model.SETSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface INotificationService
    {
        Notification_Log Create(CreateNotificationRequest req);
        UnreadNotificationsResult GetForUser(string userID, string? category, string? sectionCode, bool isAdmin);
        void MarkRead(int notifID);
        void MarkAllRead(string userID, string? category, string? sectionCode, bool isAdmin);
    }
}