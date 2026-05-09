using Model.Main;
using Model.SETSDB;
using Reposi.Context;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Reposi.Repositories
{
    public class NotificationRepo : GenericRepo<Notification_Log>
    {
        public NotificationRepo(AppDbContext context) : base(context) { }

        public Notification_Log Create(CreateNotificationRequest req)
        {
            var entity = new Notification_Log
            {
                NotifType = req.NotifType,
                Title = req.Title,
                Message = req.Message,
                TargetCategory = req.TargetCategory,
                TargetUserID = req.TargetUserID,
                TargetSection = req.TargetSection,
                IsAdmin = req.IsAdmin,
                ReferenceID = req.ReferenceID,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            Add(entity); // GenericRepo.Add() calls SaveChanges internally
            return entity;
        }

        public List<Notification_Log> GetForUser(
            string userID,
            string? category,
            string? sectionCode,
            bool isAdmin,
            bool unreadOnly = false)
        {
            var all = dbSet
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToList();

            if (unreadOnly)
                all = all.Where(n => !n.IsRead).ToList();

            return all.Where(n =>
            {
                if (n.IsAdmin)
                    return isAdmin;

                if (n.TargetUserID != null)
                    return n.TargetUserID == userID;

                if (n.TargetSection != null)
                    return n.TargetSection == sectionCode;

                if (n.TargetCategory != null)
                    return n.TargetCategory == category;

                return false;
            }).ToList();
        }

        public void MarkRead(int notifID)
        {
            var notif = dbSet.FirstOrDefault(n => n.NotifID == notifID);
            if (notif == null) return;
            notif.IsRead = true;
            Update(notif); // GenericRepo.Update() calls SaveChanges internally
        }

        public void MarkAllRead(
            string userID,
            string? category,
            string? sectionCode,
            bool isAdmin)
        {
            var unread = GetForUser(userID, category, sectionCode, isAdmin, unreadOnly: true);
            foreach (var n in unread)
            {
                n.IsRead = true;
                Update(n);
            }
        }
    }
}