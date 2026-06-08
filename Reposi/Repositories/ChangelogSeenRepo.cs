using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class ChangelogSeenRepo : GenericRepo<App_Changelog_Seen>
    {
        public ChangelogSeenRepo(AppDbContext context) : base(context) { }

        public bool HasSeen(string userID, string version)
            => dbSet.Any(s => s.UserID == userID && s.Version == version);

        public void MarkSeen(string userID, string version)
        {
            // Guard: don't insert duplicate (shouldn't happen, but safe)
            if (HasSeen(userID, version)) return;

            Add(new App_Changelog_Seen
            {
                UserID = userID,
                Version = version,
                SeenAt = DateTime.Now
            });
        }
    }
}