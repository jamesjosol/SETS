using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ISpecimenSectionService
    {
        // Header
        List<Specimen_Section_Header> GetPendingBySection(string sectionCode);
        List<Specimen_Section_Header> GetSavedBySection(string sectionCode);
        Specimen_Section_Header? GetBySpecimenAndGroup(string specimenNo, string testGroupCode);
        List<Specimen_Section_Header> GetBySpecimenNo(string specimenNo);
        bool HeaderExists(string specimenNo, string testGroupCode);
        void AddHeader(Specimen_Section_Header header);
        void UpdateHeader(Specimen_Section_Header header);
        Specimen_Section_Header? GetHeaderById(int id);

        // Tests
        List<Specimen_Section_Test> GetTestsByHeaderId(int headerId);
        List<Specimen_Section_Test> GetTestsByAssignedRMT(string userID);
        List<Specimen_Section_Test> GetSavedTestsDueToday(DateOnly today);
        void AddTest(Specimen_Section_Test test);
        void UpdateTest(Specimen_Section_Test test);
        bool AllTestsReleased(int headerId);


        // Middleware
        void ReleaseScheduledHeaders(List<int> headerId);
        List<string> GetUnroutedSpecimenNos();
        void FlipHclabRouted(string specimenNo);
        List<Specimen_Section_Test> GetAllRunningTests();
        void MarkTestReleased(int testId);
        bool TryCompleteHeader(int headerId);

    }
}