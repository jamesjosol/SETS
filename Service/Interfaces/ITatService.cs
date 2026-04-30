using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ITatService
    {
        // ── Settings ─────────────────────────────────────────────────────────
        List<Tat_Section> GetAll();
        Tat_Section? GetBySection(string sectionCode);
        void Upsert(Tat_Section tat, string userID);

        // ── Cycle ─────────────────────────────────────────────────────────────
        Tat_Cycle_Log? GetOpenCycle(string sectionCode);
        void OpenCycle(string sectionCode, DateTime cycleStart);
        void CloseCycle(string sectionCode, DateTime cycleEnd, string result, string? batchNo, string? appealedBy);
        void ResetOvernight(string sectionCode);

        // ── Batch hook ────────────────────────────────────────────────────────
        // Called when a batch is endorsed — evaluates TAT, closes current cycle,
        // flags the batch if outside TAT, opens the next cycle.
        bool EvaluateAndCycle(string sectionCode, string batchNo, DateTime endorsedAt);

        // ── Appeal ────────────────────────────────────────────────────────────
        bool CanAppeal(string sectionCode);
        void Appeal(string sectionCode, string userID, DateTime appealedAt);

        // ── Processing TAT ────────────────────────────────────────────────────────
        Tat_Processing? GetProcessingTat();
        void UpsertProcessingTat(int hours, int minutes, string userID);
        bool EvaluateProcessingTat(string batchNo, DateTime procReceived, DateTime completed);
    }
}