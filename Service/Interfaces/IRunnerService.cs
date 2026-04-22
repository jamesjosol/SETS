using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface IRunnerService
    {
        ScanSpecimenResponse ScanSpecimen(ScanSpecimenRequest request);
        void SaveAssignments(SaveAssignmentsRequest request);
        List<PendingSpecimenItem> GetPendingSpecimens(string sectionCode);
        List<ScheduledSpecimenItem> GetScheduledSpecimens(string sectionCode);
        List<Specimen_Section_Test> GetTestsByHeader(int headerId);
        List<RunningSpecimenItem> GetRunningSpecimens(string sectionCode, string? userID);
        RunnerDashboardSummary GetDashboardSummary(string sectionCode, string userID);
        List<LabSectionSummary> GetAllSectionsSummary();
        List<SectionRunningGroup> GetAllSectionsRunning();
        List<SectionPendingGroup> GetAllSectionsRecentlyRouted();
        List<SectionScheduledGroup> GetAllSectionsDueToday();
        List<SectionPendingGroup> GetAllSectionsCompletedToday();
        List<SectionPendingGroup> GetAllSectionsPending();
        List<SectionScheduledGroup> GetAllSectionsScheduled();
    }
}