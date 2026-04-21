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
        List<Specimen_Section_Header> GetPendingSpecimens(string sectionCode);
        List<Specimen_Section_Test> GetSavedTests(string sectionCode);
        List<Specimen_Section_Test> GetTestsByHeader(int headerId);
    }
}