using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;

namespace Service.Interfaces
{
    public interface ITatOutboundService
    {
        // ── Settings (Branch_Settings flags) ─────────────────────────────────
        bool IsOutboundTatEnabled();
        bool IsAppealEnabled();
        void SetOutboundTatEnabled(bool enabled, string userID);
        void SetAppealEnabled(bool enabled, string userID);

        // ── Windows ───────────────────────────────────────────────────────────
        List<Tat_Outbound_Window> GetWindows();
        void UpsertWindows(List<Tat_Outbound_Window> windows, string userID);
        void DeleteWindow(int id);

        // ── Runtime ───────────────────────────────────────────────────────────
        // Returns the active window covering 'now', or null if between windows
        Tat_Outbound_Window? GetCurrentWindow(DateTime now);
        // Returns the next upcoming window after 'now' for today's schedule, or null
        Tat_Outbound_Window? GetNextWindow(DateTime now);
        // Returns all windows for today's schedule type (Weekday or Sunday)
        List<Tat_Outbound_Window> GetTodayWindows(DateTime now);

        // ── Batch evaluation ──────────────────────────────────────────────────
        // Called at endorse time — returns true if outside all windows
        bool EvaluateOutboundBatch(DateTime endorsedAt);

        // ── Log ───────────────────────────────────────────────────────────────
        // Called by middleware when a window closes
        void LogWindowResult(int windowId, DateTime windowDate, DateTime windowStart,
                             DateTime windowEnd, string result, List<string> batchNos);
        // Append a batch no to an existing Within log for this window+date
        void AppendBatchToLog(int windowId, DateTime windowDate, string batchNo);
        // Check if a log already exists for this window+date
        Tat_Outbound_Log? GetLog(int windowId, DateTime windowDate);
        // Get logs for audit trail
        List<Tat_Outbound_Log> GetLogs(DateTime dateFrom, DateTime dateTo);

        // ── Appeal ────────────────────────────────────────────────────────────
        void Appeal(int logId, string userID, DateTime appealedAt);
        Tat_Outbound_Log? GetLogById(int logId);
    }
}