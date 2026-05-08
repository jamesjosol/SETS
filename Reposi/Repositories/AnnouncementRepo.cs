using System;
using System.Collections.Generic;
using System.Linq;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class AnnouncementRepo : GenericRepo<Announcement>
    {
        public AnnouncementRepo(AppDbContext context) : base(context) { }

        public Announcement? GetActive(DateTime now)
            => dbSet.FirstOrDefault(a => a.IsActive && a.ExpiresAt > now);

        public List<Announcement> GetAllOrdered()
            => dbSet.OrderByDescending(a => a.CreatedAt).ToList();

        public void DeactivateAll(string deactivatedBy, DateTime now)
        {
            var active = dbSet.Where(a => a.IsActive).ToList();
            foreach (var a in active)
            {
                a.IsActive = false;
                a.DeactivatedBy = deactivatedBy;
                a.DeactivatedAt = now;
                Update(a);
            }
        }
    }
}