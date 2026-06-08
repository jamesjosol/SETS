using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class ChangelogRepo : GenericRepo<App_Changelog>
    {
        private readonly AppDbContext _context;

        public ChangelogRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        // ── Entries ───────────────────────────────────────────────────────────

        /// <summary>All changelog entries ordered newest-first.</summary>
        public List<App_Changelog> GetAllOrdered()
            => dbSet.OrderByDescending(c => c.ReleasedAt).ToList();

        /// <summary>The single most recent entry by ReleasedAt.</summary>
        public App_Changelog? GetLatest()
            => dbSet.OrderByDescending(c => c.ReleasedAt).FirstOrDefault();

        public bool VersionExists(string version)
            => dbSet.Any(c => c.Version == version);

        // ── Items ─────────────────────────────────────────────────────────────

        public List<App_Changelog_Item> GetItemsByChangelogId(int changelogId)
            => _context.Set<App_Changelog_Item>()
                       .Where(i => i.ChangelogId == changelogId)
                       .OrderBy(i => i.SortOrder)
                       .ToList();

        public void AddItem(App_Changelog_Item item)
        {
            _context.Set<App_Changelog_Item>().Add(item);
            _context.SaveChanges();
        }

        public void DeleteItemsByChangelogId(int changelogId)
        {
            var items = _context.Set<App_Changelog_Item>()
                                .Where(i => i.ChangelogId == changelogId)
                                .ToList();
            if (items.Count == 0) return;
            _context.Set<App_Changelog_Item>().RemoveRange(items);
            _context.SaveChanges();
        }
    }
}