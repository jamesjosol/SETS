using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class SectionService : ISectionService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public SectionService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<Section_Master> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetAll().ToList();
            }
            catch { throw; }
        }

        public List<Section_Master> GetActive()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetActive();
            }
            catch { throw; }
        }

        public Section_Master? GetByCode(string code)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetByCode(code);
            }
            catch { throw; }
        }

        public List<Section_Master> GetByBranch(string branchCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetByBranch(branchCode);
            }
            catch { throw; }
        }

        public void Add(Section_Master section)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Sections.Add(section);
            }
            catch { throw; }
        }

        public void Update(Section_Master section)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Sections.Update(section);
            }
            catch { throw; }
        }
    }
}