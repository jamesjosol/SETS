using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.Main;

namespace Service.Interfaces
{
    public interface IContingencyService
    {
        // Config
        ContingencyConfigResponse GetConfig();
        void UpsertConfig(UpsertContingencyConfigRequest request, string userID);
        bool ValidateMasterPassword(string password);

        // Endorse (Endorser side)
        string Endorse(ContingencyEndorseRequest request, string userID, string branch, string sectionCode);
        byte[] ExportBatchExcel(string batchNo);
        List<ContingencyBatchListItem> GetBatchesByEndorsingBranch(string branchCode);
        // Batch list (Receiver side)
        List<ContingencyBatchListItem> GetBatchesByBranch(string branchCode);
        ContingencyBatchDetail GetBatchDetail(int batchId);

        // Receive (Receiver side)
        ContingencyScanResponse ScanSpecimen(ContingencyScanRequest request, string userID);

        // Import (Receiver side — cross-branch)
        ContingencyImportResult ImportFromExcel(byte[] fileBytes, string branchCode, string userID);

        // Sample types
        List<SampleTypeItem> GetSampleTypes();
    }
}