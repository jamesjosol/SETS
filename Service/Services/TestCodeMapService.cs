using System;
using System.Collections.Generic;
using System.Linq;
using HCLAB;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class TestCodeMapService : ITestCodeMapService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public TestCodeMapService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<TestCodeMapItem> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                return unit.TestCodeMaps.GetAll()
                    .Select(m => new TestCodeMapItem
                    {
                        Id = m.Id,
                        CodeA = m.CodeA,
                        CodeB = m.CodeB,
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

        public string ResolveCode(string code)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TestCodeMaps.ResolveCode(code);
            }
            catch { throw; }
        }

        public void Add(AddTestCodeMapRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // Check for duplicate — same CodeA+CodeB or CodeB+CodeA
                bool exists = unit.TestCodeMaps.GetAll().Any(m =>
                    (m.CodeA == request.CodeA && m.CodeB == request.CodeB) ||
                    (m.CodeA == request.CodeB && m.CodeB == request.CodeA));

                if (exists)
                    throw new Exception($"A mapping between '{request.CodeA}' and '{request.CodeB}' already exists.");

                unit.TestCodeMaps.Add(new Test_Code_Map
                {
                    CodeA = request.CodeA.Trim().ToUpper(),
                    CodeB = request.CodeB.Trim().ToUpper(),
                    Remarks = request.Remarks?.Trim(),
                    IsActive = true,
                    CreatedBy = request.UserID,
                    Created = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void Update(int id, UpdateTestCodeMapRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var map = unit.TestCodeMaps.FindBy(m => m.Id == id).FirstOrDefault()
                    ?? throw new Exception($"Mapping ID {id} not found.");

                bool exists = unit.TestCodeMaps.GetAll().Any(m =>
                    m.Id != id &&
                    ((m.CodeA == request.CodeA && m.CodeB == request.CodeB) ||
                     (m.CodeA == request.CodeB && m.CodeB == request.CodeA)));

                if (exists)
                    throw new Exception($"A mapping between '{request.CodeA}' and '{request.CodeB}' already exists.");

                map.CodeA = request.CodeA.Trim().ToUpper();
                map.CodeB = request.CodeB.Trim().ToUpper();
                map.Remarks = request.Remarks?.Trim();
                map.UpdatedBy = request.UserID;
                map.Updated = DateTime.Now;

                unit.TestCodeMaps.Update(map);
            }
            catch { throw; }
        }

        public void Toggle(int id, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var map = unit.TestCodeMaps.FindBy(m => m.Id == id).FirstOrDefault()
                    ?? throw new Exception($"Mapping ID {id} not found.");

                map.IsActive = !map.IsActive;
                map.UpdatedBy = userID;
                map.Updated = DateTime.Now;

                unit.TestCodeMaps.Update(map);
            }
            catch { throw; }
        }

        public void Delete(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var map = unit.TestCodeMaps.FindBy(m => m.Id == id).FirstOrDefault()
                    ?? throw new Exception($"Mapping ID {id} not found.");

                unit.TestCodeMaps.Delete(map);
            }
            catch { throw; }
        }
    }
}