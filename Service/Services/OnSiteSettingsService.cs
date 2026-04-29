using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class OnSiteSettingsService : IOnSiteSettingsService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public OnSiteSettingsService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public OnSite_Settings? GetSettings()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.OnSiteSettings.GetSettings();
            }
            catch { throw; }
        }

        public void SetEnabled(bool isEnabled, string updatedBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var settings = unit.OnSiteSettings.GetSettings();
                if (settings == null) return;
                settings.IsEnabled = isEnabled;
                settings.UpdatedBy = updatedBy;
                settings.Updated = DateTime.Now;
                unit.OnSiteSettings.Update(settings);
            }
            catch { throw; }
        }

        public List<OnSite_AllowedLabNo> GetAllAllowedLabNos()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.OnSiteAllowedLabNos.GetAll().ToList();
            }
            catch { throw; }
        }

        public void AddAllowedLabNo(string prefix, string? description, string createdBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // Guard against duplicate prefix
                var existing = unit.OnSiteAllowedLabNos.GetByPrefix(prefix);
                if (existing != null)
                    throw new Exception($"Prefix '{prefix}' already exists.");

                unit.OnSiteAllowedLabNos.Add(new OnSite_AllowedLabNo
                {
                    Prefix = prefix.Trim(),
                    Description = description?.Trim(),
                    Active = true,
                    Created = DateTime.Now,
                    CreatedBy = createdBy
                });
            }
            catch { throw; }
        }

        public void ToggleAllowedLabNo(int id, string updatedBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var entry = unit.OnSiteAllowedLabNos.Get(id)
                    ?? throw new Exception("Allowed lab no. entry not found.");
                entry.Active = !entry.Active;
                unit.OnSiteAllowedLabNos.Update(entry);
            }
            catch { throw; }
        }

        public void DeleteAllowedLabNo(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.OnSiteAllowedLabNos.Delete(id);
            }
            catch { throw; }
        }
    }
}