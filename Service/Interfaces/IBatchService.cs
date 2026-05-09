using Model.Main;
using Model.SETSDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IBatchService
    {
        EndorseResult Endorse(EndorseRequest request);
        Batch_Specimen? CheckSpecimen(string specimenNo);
        BatchSummary GetDashboardSummary(string sectionCode, DateTime date);
        List<BatchSectionSummary> GetAllSectionsSummary(DateTime date);
        List<BatchRecentItem> GetRecentBatches(string sectionCode);
        List<BatchRecentItem> GetAllSectionsRecentBatches();
        BatchDetailResponse? GetBatchDetail(string batchNo);
        List<BatchDailyFlow> GetWeeklyFlow(string sectionCode);
        List<BatchDailyFlow> GetAllSectionsWeeklyFlow();
        List<BatchRecentItem> GetEndorsements(string sectionCode, DateTime dateFrom, DateTime dateTo);
        List<BatchRecentItem> GetAllEndorsements(DateTime dateFrom, DateTime dateTo);
        List<GlobalSearchResult> GlobalSearch(string query, string userID, string sectionCategory, string sectionCode);
    }
}