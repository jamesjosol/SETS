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
    public class BranchSettingsService : IBranchSettingsService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public BranchSettingsService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public Branch_Settings Get()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.BranchSettings.Get();
            }
            catch { throw; }
        }

        public void SetOutbound(bool enabled, string updatedBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var settings = unit.BranchSettings.Get();
                settings.IsOutboundEnabled = enabled;
                settings.UpdatedAt = DateTime.Now;
                settings.UpdatedBy = updatedBy;
                unit.BranchSettings.Update(settings);
            }
            catch { throw; }
        }
    }
}
