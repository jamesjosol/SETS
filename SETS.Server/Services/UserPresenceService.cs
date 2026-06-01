using Model.Main;
using Service.Interfaces;
using System.Collections.Concurrent;

namespace SETS.Server.Services
{
    public class UserPresenceService : IUserPresenceService
    {
        // Keyed by SignalR ConnectionId — one entry per connection.
        // ConcurrentDictionary keeps this thread-safe without explicit locks.
        private readonly ConcurrentDictionary<string, UserPresenceEntry> _connections = new();

        public void Register(UserPresenceEntry entry)
        {
            _connections[entry.ConnectionId] = entry;
        }

        public void Unregister(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public IReadOnlyList<UserPresenceEntry> GetOnlineUsers(string branchCode)
        {
            return _connections.Values
                .Where(e => e.BranchCode.Equals(branchCode, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.ConnectedAt)
                .ToList()
                .AsReadOnly();
        }

        public UserPresenceEntry? GetByConnectionId(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var entry);
            return entry;
        }
    }
}