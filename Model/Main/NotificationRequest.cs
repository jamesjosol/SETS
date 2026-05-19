using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model.Main
{
    // ── Constants ─────────────────────────────────────────────────────────────
    public static class NotifType
    {
        public const string BatchEndorsed = "BATCH_ENDORSED";
        public const string BatchReceived = "BATCH_RECEIVED";
        public const string SpecimenCancelled = "SPECIMEN_CANCELLED";
        public const string SpecimenReEndorsed = "SPECIMEN_REENDORSED";
        public const string SpecimenArrived = "SPECIMEN_ARRIVED";
        public const string SpecimenCompleted = "SPECIMEN_COMPLETED";
        public const string MiddlewareIssue = "MIDDLEWARE_ISSUE";
        public const string SpecimenFlagged = "SPECIMEN_FLAGGED";
    }

    public class NotificationDto
    {
        public int NotifID { get; set; }
        public string NotifType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public string? ReferenceID { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateNotificationRequest
    {
        public string NotifType { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string? TargetCategory { get; set; }
        public string? TargetUserID { get; set; }
        public string? TargetSection { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string? ReferenceID { get; set; }
    }

    public class UnreadNotificationsResult
    {
        public List<NotificationDto> Items { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}