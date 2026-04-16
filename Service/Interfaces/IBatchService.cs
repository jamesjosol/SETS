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
        string Endorse(Model.Main.EndorseRequest request);
        Batch_Specimen? CheckSpecimen(string specimenNo);
        BatchSummary GetDashboardSummary(string sectionCode, DateTime date);
        List<BatchSectionSummary> GetAllSectionsSummary(DateTime date);
        List<BatchRecentItem> GetRecentBatches(string sectionCode, int top = 5);
        List<BatchRecentItem> GetAllSectionsRecentBatches(int top = 5);
        BatchDetailResponse? GetBatchDetail(string batchNo);
        List<BatchDailyFlow> GetWeeklyFlow(string sectionCode);
        List<BatchDailyFlow> GetAllSectionsWeeklyFlow();
    }
}