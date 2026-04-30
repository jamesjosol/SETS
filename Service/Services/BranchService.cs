using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class BranchService : IBranchService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public BranchService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<Branch_Master> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Branches.GetAll().ToList();
            }
            catch { throw; }
        }

        public List<Branch_Master> GetActive()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Branches.GetActive();
            }
            catch { throw; }
        }

        public Branch_Master? GetByCode(string code)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Branches.GetByCode(code);
            }
            catch { throw; }
        }

        public void Add(Branch_Master branch)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Branches.Add(branch);
            }
            catch { throw; }
        }

        public void Update(Branch_Master branch)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Branches.Update(branch);
            }
            catch { throw; }
        }

        public bool CodeExists(string code)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Branches.GetByCode(code) != null;
            }
            catch { throw; }
        }

        public void Toggle(string code, string updatedBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var branch = unit.Branches.GetByCode(code)
                    ?? throw new Exception($"Branch '{code}' not found.");
                branch.Active = !branch.Active;
                branch.Updated = DateTime.Now;
                branch.UpdatedBy = updatedBy;
                unit.Branches.Update(branch);
            }
            catch { throw; }
        }
    }
}