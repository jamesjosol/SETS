using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public NotificationService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public Notification_Log Create(CreateNotificationRequest req)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Notifications.Create(req);
            }
            catch { throw; }
        }

        public UnreadNotificationsResult GetForUser(
            string userID,
            string? category,
            string? sectionCode,
            bool isAdmin)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var items = unit.Notifications.GetForUser(userID, category, sectionCode, isAdmin);

                return new UnreadNotificationsResult
                {
                    Items = items.Select(n => new NotificationDto
                    {
                        NotifID = n.NotifID,
                        NotifType = n.NotifType,
                        Title = n.Title,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        ReferenceID = n.ReferenceID,
                        CreatedAt = n.CreatedAt,
                    }).ToList(),
                    UnreadCount = items.Count(n => !n.IsRead)
                };
            }
            catch { throw; }
        }

        public void MarkRead(int notifID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Notifications.MarkRead(notifID);
            }
            catch { throw; }
        }

        public void MarkAllRead(
            string userID,
            string? category,
            string? sectionCode,
            bool isAdmin)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Notifications.MarkAllRead(userID, category, sectionCode, isAdmin);
            }
            catch { throw; }
        }
    }
}