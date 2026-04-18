using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Reposi.Context;
using Service.Interfaces;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using Reposi;
using HCLAB;

namespace Service.Services
{
    public class HealthService : IHealthService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string branch_raw;

        public HealthService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            branch_raw = branch;
        }

        public HealthCheckResult CheckDatabase()
        {
            var sw = Stopwatch.StartNew();
            try
            {
                using var context = _factory.CreateContext(_branch);
                context.Database.OpenConnection();
                context.Database.CloseConnection();
                sw.Stop();
                return new HealthCheckResult { Online = true, LatencyMs = sw.ElapsedMilliseconds };
            }
            catch (Exception ex)
            {
                sw.Stop();
                return new HealthCheckResult { Online = false, LatencyMs = sw.ElapsedMilliseconds, Error = ex.Message };
            }
        }

        public HealthCheckResult CheckHcLab()
        {
            HealthCheckResult res = HclabMaster.MISC.CheckHcLab(HclabConnection.ConnectionString(branch_raw));
            return res;
        }
    }
}