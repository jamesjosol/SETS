using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class PCService : IPCService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public PCService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<PC_Master> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.PCs.GetAll().ToList();
            }
            catch { throw; }
        }

        public PC_Master? GetByIP(string ipAddress)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.PCs.GetByIP(ipAddress);
            }
            catch { throw; }
        }

        public List<PC_Section> GetSectionsByIP(string ipAddress)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var pc = unit.PCs.GetByIP(ipAddress);
                if (pc == null) return new List<PC_Section>();
                return unit.PCSections.GetByPCId(pc.Id);
            }
            catch { throw; }
        }

        public List<PC_Section> GetSectionsByPCId(int pcId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.PCSections.GetByPCId(pcId);
            }
            catch { throw; }
        }

        public void Add(PC_Master pc)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.PCs.Add(pc);
            }
            catch { throw; }
        }

        public void Update(PC_Master pc)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.PCs.Update(pc);
            }
            catch { throw; }
        }

        // ── Delete PC master record ───────────────────────────────────────────
        public void Delete(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                // Remove all section assignments first to avoid FK violations
                var sections = unit.PCSections.GetByPCId(id);
                foreach (var s in sections)
                    unit.PCSections.Delete(s);
                unit.PCs.Delete(id);
            }
            catch { throw; }
        }

        public void AddSection(PC_Section pcSection)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.PCSections.Add(pcSection);
            }
            catch { throw; }
        }

        public void DeleteSection(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.PCSections.Delete(id);
            }
            catch { throw; }
        }

        public void DeleteSectionsByPCId(int pcId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var rows = unit.PCSections.GetByPCId(pcId);
                foreach (var row in rows)
                    unit.PCSections.Delete(row);
            }
            catch { throw; }
        }
    }
}