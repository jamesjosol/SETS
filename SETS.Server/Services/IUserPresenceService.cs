namespace SETS.Server.Services
{
    public class UserPresenceEntry
    {
        public string ConnectionId { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string SectionCode { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;  // "1" | "2" | "3" | "admin"
        public bool IsAdmin { get; set; }
        public string? ProfilePicture { get; set; }                 // base64 or null
        public DateTime ConnectedAt { get; set; }
    }

    public interface IUserPresenceService
    {
        void Register(UserPresenceEntry entry);
        void Unregister(string connectionId);
        IReadOnlyList<UserPresenceEntry> GetOnlineUsers(string branchCode);
        UserPresenceEntry? GetByConnectionId(string connectionId);
    }
}