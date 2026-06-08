using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;

namespace Service.Services
{
    public class ChangelogService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public ChangelogService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>
        /// All changelog entries with their items, newest-first.
        /// Used by the admin management tab.
        /// </summary>
        public List<ChangelogEntryDto> GetAll()
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            var entries = unit.Changelog.GetAllOrdered();
            return entries.Select(e => ToDto(e, unit)).ToList();
        }

        /// <summary>
        /// The latest changelog entry with its items.
        /// Used by the frontend on login to decide whether to show the modal.
        /// Returns null if no entries exist.
        /// </summary>
        public ChangelogEntryDto? GetLatest()
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            var entry = unit.Changelog.GetLatest();
            if (entry == null) return null;

            return ToDto(entry, unit);
        }

        /// <summary>
        /// Returns true if the given user has already seen the given version.
        /// </summary>
        public bool HasSeen(string userID, string version)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            return unit.ChangelogSeen.HasSeen(userID, version);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new changelog entry with its items.
        /// Throws if the version already exists.
        /// </summary>
        public ChangelogEntryDto Create(CreateChangelogRequest req, string createdBy)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            if (unit.Changelog.VersionExists(req.Version.Trim()))
                throw new InvalidOperationException($"Version {req.Version} already exists.");

            var entry = new App_Changelog
            {
                Version = req.Version.Trim(),
                Title = req.Title.Trim(),
                ReleasedAt = req.ReleasedAt,
                CreatedBy = createdBy,
                CreatedAt = DateTime.Now
            };

            unit.Changelog.Add(entry);

            // Items — re-open context after Add() so entry.Id is populated
            using var context2 = _factory.CreateContext(_branch);
            using var unit2 = new UnitOfWork(context2);

            // Fetch the saved entry to get its Id
            var saved = unit2.Changelog.GetAllOrdered().First(c => c.Version == entry.Version);

            for (int i = 0; i < req.Items.Count; i++)
            {
                var item = req.Items[i];
                unit2.Changelog.AddItem(new App_Changelog_Item
                {
                    ChangelogId = saved.Id,
                    Tag = item.Tag.Trim().ToUpper(),
                    Description = item.Description.Trim(),
                    SortOrder = item.SortOrder > 0 ? item.SortOrder : i
                });
            }

            return ToDto(saved, unit2);
        }

        /// <summary>
        /// Deletes a changelog entry and all its items (cascade).
        /// </summary>
        public void Delete(int id)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            var entry = unit.Changelog.Get(id)
                ?? throw new KeyNotFoundException($"Changelog entry {id} not found.");

            // Items are cascade-deleted by the DB FK, but we delete explicitly
            // to be safe in case EF doesn't track them.
            unit.Changelog.DeleteItemsByChangelogId(id);

            using var context2 = _factory.CreateContext(_branch);
            using var unit2 = new UnitOfWork(context2);
            var fresh = unit2.Changelog.Get(id);
            if (fresh != null) unit2.Changelog.Delete(fresh);
        }

        /// <summary>
        /// Records that a user has seen a version's changelog.
        /// Idempotent — safe to call multiple times.
        /// </summary>
        public void MarkSeen(string userID, string version)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            unit.ChangelogSeen.MarkSeen(userID, version);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static ChangelogEntryDto ToDto(App_Changelog entry, UnitOfWork unit)
        {
            var items = unit.Changelog.GetItemsByChangelogId(entry.Id);
            return new ChangelogEntryDto
            {
                Id = entry.Id,
                Version = entry.Version,
                Title = entry.Title,
                ReleasedAt = entry.ReleasedAt,
                CreatedBy = entry.CreatedBy,
                CreatedAt = entry.CreatedAt,
                Items = items.Select(i => new ChangelogItemDto
                {
                    Id = i.Id,
                    Tag = i.Tag,
                    Description = i.Description,
                    SortOrder = i.SortOrder
                }).ToList()
            };
        }
    }
}