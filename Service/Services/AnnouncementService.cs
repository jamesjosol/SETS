using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public AnnouncementService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public AnnouncementDto? GetActive()
        {
            try
            {
                var now = DateTime.Now;
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var a = unit.Announcement.GetActive(now);
                return a == null ? null : MapToDto(a);
            }
            catch { throw; }
        }

        public List<AnnouncementDto> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Announcement.GetAllOrdered().Select(MapToDto).ToList();
            }
            catch { throw; }
        }

        public void Create(CreateAnnouncementRequest req)
        {
            try
            {
                var now = DateTime.Now;
                var expiresAt = ResolveExpiry(req, now);

                if (expiresAt <= now)
                    throw new Exception("Expiry must be in the future.");

                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                unit.Announcement.DeactivateAll(req.CreatedBy, now);

                unit.Announcement.Add(new Announcement
                {
                    Title = req.Title?.Trim(),
                    Message = req.Message.Trim(),
                    Type = req.Type,
                    TargetRoles = req.TargetRoles,
                    ExpiresAt = expiresAt,
                    IsActive = true,
                    CreatedBy = req.CreatedBy,
                    CreatedAt = now,
                });
            }
            catch { throw; }
        }

        public void Deactivate(int id, string deactivatedBy)
        {
            try
            {
                var now = DateTime.Now;
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var a = unit.Announcement.Get(id)
                    ?? throw new Exception("Announcement not found.");

                if (!a.IsActive)
                    throw new Exception("Announcement is already inactive.");

                a.IsActive = false;
                a.DeactivatedBy = deactivatedBy;
                a.DeactivatedAt = now;

                unit.Announcement.Update(a);
            }
            catch { throw; }
        }

        // ── Private helpers ───────────────────────────────────────────────────

        private static DateTime ResolveExpiry(CreateAnnouncementRequest req, DateTime now)
        {
            return req.DurationMode switch
            {
                "datetime" => req.ExpiresAt
                                ?? throw new Exception("ExpiresAt is required for datetime mode."),
                "hours" => now.AddHours(req.DurationValue
                                ?? throw new Exception("DurationValue is required.")),
                "minutes" => now.AddMinutes(req.DurationValue
                                ?? throw new Exception("DurationValue is required.")),
                "seconds" => now.AddSeconds(req.DurationValue
                                ?? throw new Exception("DurationValue is required.")),
                _ => throw new Exception($"Unknown DurationMode: {req.DurationMode}")
            };
        }

        private static AnnouncementDto MapToDto(Announcement a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Message = a.Message,
            Type = a.Type,
            TargetRoles = a.TargetRoles,
            ExpiresAt = a.ExpiresAt,
            IsActive = a.IsActive,
            CreatedBy = a.CreatedBy,
            CreatedAt = a.CreatedAt,
        };
    }
}