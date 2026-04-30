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
    public class ProcessingOptionsService : IProcessingOptionsService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branchCode;

        public ProcessingOptionsService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branchCode = branch;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public Processing_Options GetOptions()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // Return stored options or defaults if not yet configured
                return unit.ProcessingOptions.Get(_branchCode) ?? new Processing_Options
                {
                    BranchCode = _branchCode,
                    ShowTemperature = true,
                    ShowTempRemarks = true,
                    ShowBagNo = true
                };
            }
            catch { throw; }
        }

        public void UpsertOptions(bool showTemperature, bool showTempRemarks, bool showBagNo, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var existing = unit.ProcessingOptions.Get(_branchCode);
                if (existing == null)
                {
                    unit.ProcessingOptions.Add(new Processing_Options
                    {
                        BranchCode = _branchCode,
                        ShowTemperature = showTemperature,
                        ShowTempRemarks = showTempRemarks,
                        ShowBagNo = showBagNo,
                        UpdatedBy = userID,
                        Updated = DateTime.Now
                    });
                }
                else
                {
                    existing.ShowTemperature = showTemperature;
                    existing.ShowTempRemarks = showTempRemarks;
                    existing.ShowBagNo = showBagNo;
                    existing.UpdatedBy = userID;
                    existing.Updated = DateTime.Now;
                    unit.ProcessingOptions.Update(existing);
                }

                context.SaveChanges();
            }
            catch { throw; }
        }
    }
}