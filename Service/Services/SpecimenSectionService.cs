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
    public class SpecimenSectionService : ISpecimenSectionService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public SpecimenSectionService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<Specimen_Section_Header> GetPendingBySection(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionHeaders.GetPendingBySection(sectionCode);
            }
            catch { throw; }
        }

        public List<Specimen_Section_Header> GetSavedBySection(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionHeaders.GetSavedBySection(sectionCode);
            }
            catch { throw; }
        }

        public Specimen_Section_Header? GetBySpecimenAndGroup(string specimenNo, string testGroupCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionHeaders.GetBySpecimenAndGroup(specimenNo, testGroupCode);
            }
            catch { throw; }
        }

        public List<Specimen_Section_Header> GetBySpecimenNo(string specimenNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionHeaders.GetBySpecimenNo(specimenNo);
            }
            catch { throw; }
        }

        public bool HeaderExists(string specimenNo, string testGroupCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionHeaders.Exists(specimenNo, testGroupCode);
            }
            catch { throw; }
        }

        public void AddHeader(Specimen_Section_Header header)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SpecimenSectionHeaders.Add(header);
            }
            catch { throw; }
        }

        public void UpdateHeader(Specimen_Section_Header header)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SpecimenSectionHeaders.Update(header);
            }
            catch { throw; }
        }

        public List<Specimen_Section_Test> GetTestsByHeaderId(int headerId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionTests.GetByHeaderId(headerId);
            }
            catch { throw; }
        }

        public List<Specimen_Section_Test> GetTestsByAssignedRMT(string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionTests.GetByAssignedRMT(userID);
            }
            catch { throw; }
        }

        public List<Specimen_Section_Test> GetSavedTestsDueToday(DateOnly today)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionTests.GetSavedDueToday(today);
            }
            catch { throw; }
        }

        public void AddTest(Specimen_Section_Test test)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SpecimenSectionTests.Add(test);
            }
            catch { throw; }
        }

        public void UpdateTest(Specimen_Section_Test test)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.SpecimenSectionTests.Update(test);
            }
            catch { throw; }
        }

        public bool AllTestsReleased(int headerId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.SpecimenSectionTests.AllCompleted(headerId);
            }
            catch { throw; }
        }
    }
}