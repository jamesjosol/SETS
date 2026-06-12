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
    public class TestGroupOverrideService : ITestGroupOverrideService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public TestGroupOverrideService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<TestGroupOverrideItem> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                return unit.TestGroupOverrides.GetAll()
                    .Select(m => new TestGroupOverrideItem
                    {
                        Id = m.Id,
                        TestCode = m.TestCode,
                        OverrideGroup = m.OverrideGroup,
                        Remarks = m.Remarks,
                        IsActive = m.IsActive,
                        CreatedBy = m.CreatedBy,
                        Created = m.Created,
                        UpdatedBy = m.UpdatedBy,
                        Updated = m.Updated
                    }).ToList();
            }
            catch { throw; }
        }

        public string? ResolveGroup(string testCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestGroupOverrides.ResolveGroup(testCode);
            }
            catch { throw; }
        }

        public void Add(AddTestGroupOverrideRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                bool exists = unit.TestGroupOverrides.GetAll()
                    .Any(m => m.TestCode == request.TestCode.Trim().ToUpper());

                if (exists)
                    throw new Exception($"An override for test code '{request.TestCode}' already exists.");

                unit.TestGroupOverrides.Add(new Test_Group_Override
                {
                    TestCode = request.TestCode.Trim().ToUpper(),
                    OverrideGroup = request.OverrideGroup.Trim().ToUpper(),
                    Remarks = request.Remarks?.Trim(),
                    IsActive = true,
                    CreatedBy = request.UserID,
                    Created = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void Update(int id, UpdateTestGroupOverrideRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var item = unit.TestGroupOverrides.FindBy(m => m.Id == id).FirstOrDefault()
                    ?? throw new Exception($"Override ID {id} not found.");

                // Check duplicate — exclude self
                bool exists = unit.TestGroupOverrides.GetAll()
                    .Any(m => m.Id != id &&
                              m.TestCode == request.TestCode.Trim().ToUpper());

                if (exists)
                    throw new Exception($"An override for test code '{request.TestCode}' already exists.");

                item.TestCode = request.TestCode.Trim().ToUpper();
                item.OverrideGroup = request.OverrideGroup.Trim().ToUpper();
                item.Remarks = request.Remarks?.Trim();
                item.UpdatedBy = request.UserID;
                item.Updated = DateTime.Now;

                unit.TestGroupOverrides.Update(item);
            }
            catch { throw; }
        }

        public void Toggle(int id, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var item = unit.TestGroupOverrides.FindBy(m => m.Id == id).FirstOrDefault()
                    ?? throw new Exception($"Override ID {id} not found.");

                item.IsActive = !item.IsActive;
                item.UpdatedBy = userID;
                item.Updated = DateTime.Now;

                unit.TestGroupOverrides.Update(item);
            }
            catch { throw; }
        }

        public void Delete(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var item = unit.TestGroupOverrides.FindBy(m => m.Id == id).FirstOrDefault()
                    ?? throw new Exception($"Override ID {id} not found.");

                unit.TestGroupOverrides.Delete(item);
            }
            catch { throw; }
        }
    }
}