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

        public void UpsertFromHclab(List<Model.HCLAB.Ref_Tables> records)
        {
            using var context = _factory.CreateContext(_branch);
            using var unit = new UnitOfWork(context);

            foreach (var record in records)
            {
                var existing = unit.TestGroups.GetByCode(record.Code);
                if (existing == null)
                {
                    unit.TestGroups.Add(new Model.SETSDB.Test_Group
                    {
                        Code = record.Code,
                        Name = record.Name
                    });
                }
                else
                {
                    existing.Name = record.Name;
                    unit.TestGroups.Update(existing);
                }
            }
        }
    }
}