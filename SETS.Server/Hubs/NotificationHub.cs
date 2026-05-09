using Microsoft.AspNetCore.SignalR;

namespace SETS.Server.Hubs
{
    public class NotificationHub : Hub
    {
        // Join branch-wide group (all roles)
        public async Task JoinBranch(string branchCode)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"notif-branch-{branchCode}");

        public async Task LeaveBranch(string branchCode)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notif-branch-{branchCode}");

        // Join category group (e.g. notif-cat-1, notif-cat-2, notif-cat-3)
        public async Task JoinCategory(string branchCode, string category)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"notif-{branchCode}-cat-{category}");

        public async Task LeaveCategory(string branchCode, string category)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notif-{branchCode}-cat-{category}");

        // Join section group (for runner section-scoped notifs)
        public async Task JoinSection(string branchCode, string sectionCode)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"notif-{branchCode}-sec-{sectionCode}");

        public async Task LeaveSection(string branchCode, string sectionCode)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notif-{branchCode}-sec-{sectionCode}");

        // Join personal group (for user-targeted notifs)
        public async Task JoinUser(string userID)
            => await Groups.AddToGroupAsync(Context.ConnectionId, $"notif-user-{userID}");

        public async Task LeaveUser(string userID)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"notif-user-{userID}");
    }
}