using Microsoft.AspNetCore.SignalR;

namespace SETS.Server.Hubs
{
    public class ContingencyHub : Hub
    {
        // Receiver joins a group scoped to their branch
        public async Task JoinBranch(string branchCode)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"branch-{branchCode}");
        }

        public async Task LeaveBranch(string branchCode)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"branch-{branchCode}");
        }
    }
}