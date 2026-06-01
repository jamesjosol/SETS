using Microsoft.AspNetCore.SignalR;
using SETS.Server.Services;

namespace SETS.Server.Hubs
{
    public class PresenceHub : Hub
    {
        private readonly IUserPresenceService _presence;

        public PresenceHub(IUserPresenceService presence)
        {
            _presence = presence;
        }

        // ── Called by the frontend immediately after connection ───────────────
        public async Task Register(
            string userID,
            string userName,
            string branchCode,
            string sectionCode,
            string sectionName,
            string category,
            bool isAdmin,
            string? profilePicture = null)
        {
            _presence.Register(new UserPresenceEntry
            {
                ConnectionId = Context.ConnectionId,
                UserID = userID,
                UserName = userName,
                BranchCode = branchCode,
                SectionCode = sectionCode,
                SectionName = sectionName,
                Category = isAdmin ? "admin" : category,
                IsAdmin = isAdmin,
                ProfilePicture = profilePicture,
                ConnectedAt = DateTime.Now,
            });

            // Join the branch group so this connection receives future PresenceUpdated pushes
            await Groups.AddToGroupAsync(Context.ConnectionId, $"presence-{branchCode}");

            await PushUpdate(branchCode);
        }

        // ── Called by the frontend on clean logout ────────────────────────────
        public async Task Unregister(string branchCode)
        {
            _presence.Unregister(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"presence-{branchCode}");
            await PushUpdate(branchCode);
        }

        // ── Fallback: clean up on abrupt disconnect (browser close, network drop)
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var entry = _presence.GetByConnectionId(Context.ConnectionId);
            if (entry != null)
            {
                _presence.Unregister(Context.ConnectionId);
                await PushUpdate(entry.BranchCode);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private async Task PushUpdate(string branchCode)
        {
            var users = _presence.GetOnlineUsers(branchCode)
                .Select(BuildPayload)
                .ToList();

            await Clients
                .Group($"presence-{branchCode}")
                .SendAsync("PresenceUpdated", users);
        }

        private static object BuildPayload(UserPresenceEntry e) => new
        {
            e.UserID,
            e.UserName,
            e.BranchCode,
            e.SectionCode,
            e.SectionName,
            e.Category,
            e.IsAdmin,
            e.ProfilePicture,
            connectedAt = e.ConnectedAt.ToString("yyyy-MM-ddTHH:mm:ss"),
        };
    }
}