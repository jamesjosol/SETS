using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IOnSiteService
    {
        Task<ScanOnSiteSpecimenResponse> ScanSpecimen(ScanOnSiteSpecimenRequest request);
        void SaveAssignments(SaveAssignmentsRequest request);

        // Pending / Saved / Running — mirrors RunnerService
        List<PendingSpecimenItem> GetPendingSpecimens(string sectionCode);
        List<ScheduledSpecimenItem> GetScheduledSpecimens(string sectionCode);
        List<RunningSpecimenItem> GetRunningSpecimens(string sectionCode, string? userID);
        List<OnSite_Section_Test> GetDueScheduledTests(DateOnly today);
        void ReleaseScheduledHeaders(List<int> headerIds);
        List<OnSite_Section_Test> GetAllRunningTests();
        OnSite_Section_Header? GetHeaderById(int id);
        void MarkTestReleased(int testId, string? releasedBy, DateTime? releasedOn);
        bool TryCompleteHeader(int headerId);
        List<OnSite_Section_Test> GetTestsByHeaderId(int headerId);
        void CancelSpecimen(CancelSectionSpecimenRequest request);
        void AbortRunningTest(AbortRunningTestRequest request);
    }
}