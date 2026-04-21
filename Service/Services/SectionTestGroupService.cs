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
    public class SectionTestGroupService : ISectionTestGroupService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public SectionTestGroupService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<Section_TestGroup> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SectionTestGroups.GetAll().ToList();
            }
            catch { throw; }
        }

        public List<Section_TestGroup> GetBySectionCode(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SectionTestGroups.GetBySectionCode(sectionCode);
            }
            catch { throw; }
        }

        public List<Section_TestGroup> GetActive()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SectionTestGroups.GetActive();
            }
            catch { throw; }
        }

        public Section_TestGroup? GetBySectionAndGroup(string sectionCode, string testGroupCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SectionTestGroups.GetBySectionAndGroup(sectionCode, testGroupCode);
            }
            catch { throw; }
        }

        public List<string> ResolveSectionCodes(List<string> testGroupCodes)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SectionTestGroups.ResolveSectionCodes(testGroupCodes);
            }
            catch { throw; }
        }

        public void Add(Section_TestGroup item)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SectionTestGroups.Add(item);
            }
            catch { throw; }
        }

        public void Update(Section_TestGroup item)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SectionTestGroups.Update(item);
            }
            catch { throw; }
        }

        public void Delete(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SectionTestGroups.Delete(id);
            }
            catch { throw; }
        }
    }
}