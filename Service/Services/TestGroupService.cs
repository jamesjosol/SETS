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
    public class TestGroupService : ITestGroupService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public TestGroupService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<Test_Group> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestGroups.GetAll().OrderBy(t => t.Name).ToList();
            }
            catch { throw; }
        }

        public Test_Group? GetByCode(string code)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestGroups.GetByCode(code);
            }
            catch { throw; }
        }
    }
}