using System;
using System.Collections.Generic;
using System.Linq;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class TatOutboundService : ITatOutboundService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public TatOutboundService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── Settings ──────────────────────────────────────────────────────────

        public bool IsOutboundTatEnabled()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var settings = unit.BranchSettings.Get();
                return settings?.OutboundTatEnabled ?? false;
            }
            catch { throw; }
        }

        public bool IsAppealEnabled()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var settings = unit.BranchSettings.Get();
                return settings?.OutboundTatAppealEnabled ?? true;
            }
            catch { throw; }
        }

        public void SetOutboundTatEnabled(bool enabled, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var settings = unit.BranchSettings.Get();
                if (settings == null) return;
                settings.OutboundTatEnabled = enabled;
                settings.UpdatedAt = DateTime.Now;
                settings.UpdatedBy = userID;
                unit.BranchSettings.Update(settings);
            }
            catch { throw; }
        }

        public void SetAppealEnabled(bool enabled, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var settings = unit.BranchSettings.Get();
                if (settings == null) return;
                settings.OutboundTatAppealEnabled = enabled;
                settings.UpdatedAt = DateTime.Now;
                settings.UpdatedBy = userID;
                unit.BranchSettings.Update(settings);
            }
            catch { throw; }
        }

        // ── Windows ───────────────────────────────────────────────────────────

        public List<Tat_Outbound_Window> GetWindows()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatOutboundWindows.GetAll();
            }
            catch { throw; }
        }

        public void UpsertWindows(List<Tat_Outbound_Window> windows, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                foreach (var w in windows)
                {
                    if (w.Id == 0)
                    {
                        w.CreatedBy = userID;
                        w.Created = DateTime.Now;
                        w.UpdatedBy = null;
                        w.Updated = null;
                        unit.TatOutboundWindows.Add(w);
                    }
                    else
                    {
                        var existing = unit.TatOutboundWindows.GetById(w.Id);
                        if (existing == null) continue;
                        existing.WindowStart = w.WindowStart;
                        existing.WindowEnd = w.WindowEnd;
                        existing.ScheduleType = w.ScheduleType;
                        existing.IsActive = w.IsActive;
                        existing.UpdatedBy = userID;
                        existing.Updated = DateTime.Now;
                        unit.TatOutboundWindows.Update(existing);
                    }
                }
            }
            catch { throw; }
        }

        public void DeleteWindow(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var existing = unit.TatOutboundWindows.GetById(id);
                if (existing == null) return;
                unit.TatOutboundWindows.Delete(existing);
            }
            catch { throw; }
        }

        // ── Runtime ───────────────────────────────────────────────────────────

        public List<Tat_Outbound_Window> GetTodayWindows(DateTime now)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var allActive = unit.TatOutboundWindows.GetActive();

                var scheduleType = now.DayOfWeek == DayOfWeek.Sunday ? "Sunday" : "Weekday";

                // If Sunday has no configured windows, fall back to Weekday schedule
                if (scheduleType == "Sunday" && !allActive.Any(w => w.ScheduleType == "Sunday"))
                    scheduleType = "Weekday";

                return allActive
                    .Where(w => w.ScheduleType == scheduleType)
                    .OrderBy(w => w.WindowStart)
                    .ToList();
            }
            catch { throw; }
        }

        public Tat_Outbound_Window? GetCurrentWindow(DateTime now)
        {
            try
            {
                var todayWindows = GetTodayWindows(now);
                var currentTime = now.TimeOfDay;
                return todayWindows.FirstOrDefault(w =>
                    currentTime >= w.WindowStart && currentTime < w.WindowEnd);
            }
            catch { throw; }
        }

        public Tat_Outbound_Window? GetNextWindow(DateTime now)
        {
            try
            {
                var todayWindows = GetTodayWindows(now);
                var currentTime = now.TimeOfDay;
                return todayWindows.FirstOrDefault(w => w.WindowStart > currentTime);
            }
            catch { throw; }
        }

        // ── Batch evaluation ──────────────────────────────────────────────────

        public bool EvaluateOutboundBatch(DateTime endorsedAt)
        {
            try
            {
                var currentWindow = GetCurrentWindow(endorsedAt);
                return currentWindow == null;
            }
            catch { throw; }
        }

        // ── Log ───────────────────────────────────────────────────────────────

        public Tat_Outbound_Log? GetLog(int windowId, DateTime windowDate)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatOutboundLogs.GetByWindowAndDate(windowId, windowDate);
            }
            catch { throw; }
        }

        public Tat_Outbound_Log? GetLogById(int logId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatOutboundLogs.GetById(logId);
            }
            catch { throw; }
        }

        public void LogWindowResult(int windowId, DateTime windowDate, DateTime windowStart,
                                    DateTime windowEnd, string result, List<string> batchNos)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // Idempotent — skip if already logged
                var existing = unit.TatOutboundLogs.GetByWindowAndDate(windowId, windowDate);
                if (existing != null) return;

                unit.TatOutboundLogs.Add(new Tat_Outbound_Log
                {
                    WindowId = windowId,
                    WindowDate = windowDate.Date,
                    WindowStart = windowStart,
                    WindowEnd = windowEnd,
                    BatchNos = batchNos.Count > 0 ? string.Join(",", batchNos) : null,
                    Result = result,
                    Created = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void AppendBatchToLog(int windowId, DateTime windowDate, string batchNo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var existing = unit.TatOutboundLogs.GetByWindowAndDate(windowId, windowDate);
                if (existing == null) return;

                var list = string.IsNullOrEmpty(existing.BatchNos)
                    ? new List<string>()
                    : existing.BatchNos.Split(',').ToList();

                if (!list.Contains(batchNo))
                {
                    list.Add(batchNo);
                    existing.BatchNos = string.Join(",", list);
                    unit.TatOutboundLogs.Update(existing);
                }
            }
            catch { throw; }
        }

        public List<Tat_Outbound_Log> GetLogs(DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatOutboundLogs.GetByDateRange(dateFrom, dateTo);
            }
            catch { throw; }
        }

        // ── Appeal ────────────────────────────────────────────────────────────

        public void Appeal(int logId, string userID, DateTime appealedAt)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var log = unit.TatOutboundLogs.GetById(logId);
                if (log == null) return;

                // Can only appeal a Missed window
                if (log.Result != "Missed") return;

                log.Result = "Appealed";
                log.AppealedBy = userID;
                log.AppealedAt = appealedAt;
                unit.TatOutboundLogs.Update(log);
            }
            catch { throw; }
        }

        public bool HasEndorsedInWindow(Tat_Outbound_Window window, DateTime now)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                var windowStart = now.Date.Add(window.WindowStart);
                var windowEnd = now.Date.Add(window.WindowEnd);

                return context.Batch_Header
                    .Where(h => h.IsOutbound
                             && !h.IsInbound
                             && h.Endorsed >= windowStart
                             && h.Endorsed < windowEnd)
                    .ToList()
                    .Any();
            }
            catch { throw; }
        }
    }
}