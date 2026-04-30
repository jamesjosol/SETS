using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class TatService : ITatService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;  

        public TatService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── Settings ─────────────────────────────────────────────────────────

        public List<Tat_Section> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatSections.GetAll();
            }
            catch { throw; }
        }

        public Tat_Section? GetBySection(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatSections.GetBySection(sectionCode);
            }
            catch { throw; }
        }

        public void Upsert(Tat_Section tat, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var existing = unit.TatSections.GetBySection(tat.SectionCode);
                if (existing == null)
                {
                    tat.CreatedBy = userID;
                    tat.Created = DateTime.Now;
                    unit.TatSections.Add(tat);
                }
                else
                {
                    existing.Hours = tat.Hours;
                    existing.Minutes = tat.Minutes;
                    existing.AppealWindow = tat.AppealWindow;
                    existing.UpdatedBy = userID;
                    existing.Updated = DateTime.Now;
                    unit.TatSections.Update(existing);
                }
            }
            catch { throw; }
        }

        // ── Cycle ─────────────────────────────────────────────────────────────

        public Tat_Cycle_Log? GetOpenCycle(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatCycleLogs.GetOpenCycle(sectionCode);
            }
            catch { throw; }
        }

        public void OpenCycle(string sectionCode, DateTime cycleStart)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.TatCycleLogs.Add(new Tat_Cycle_Log
                {
                    SectionCode = sectionCode,
                    CycleStart = cycleStart,
                    Created = DateTime.Now
                });
            }
            catch { throw; }
        }

        public void CloseCycle(string sectionCode, DateTime cycleEnd, string result, string? batchNo, string? appealedBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var cycle = unit.TatCycleLogs.GetOpenCycle(sectionCode);
                if (cycle == null) return;

                cycle.CycleEnd = cycleEnd;
                cycle.ElapsedMinutes = (decimal)(cycleEnd - cycle.CycleStart).TotalMinutes;
                cycle.Result = result;
                cycle.BatchNo = batchNo;
                cycle.AppealedBy = appealedBy;
                unit.TatCycleLogs.Update(cycle);
            }
            catch { throw; }
        }

        public void ResetOvernight(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // Close any open cycle left from yesterday without flagging a violation
                var openCycle = unit.TatCycleLogs.GetOpenCycle(sectionCode);
                if (openCycle != null)
                {
                    var resetTime = DateTime.Today.AddHours(19); // 7:00 PM today
                    openCycle.CycleEnd = resetTime;
                    openCycle.ElapsedMinutes = (decimal)(resetTime - openCycle.CycleStart).TotalMinutes;
                    openCycle.Result = "EndOfDay";
                    unit.TatCycleLogs.Update(openCycle);
                }
            }
            catch { throw; }
        }

        // ── Batch hook ────────────────────────────────────────────────────────

        public bool EvaluateAndCycle(string sectionCode, string batchNo, DateTime endorsedAt)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var setting = unit.TatSections.GetBySection(sectionCode);
                var openCycle = unit.TatCycleLogs.GetOpenCycle(sectionCode);

                // No open cycle means this is the first batch of the day — exempt
                if (openCycle == null)
                {
                    // Open next cycle starting from this endorsement
                    unit.TatCycleLogs.Add(new Tat_Cycle_Log
                    {
                        SectionCode = sectionCode,
                        CycleStart = endorsedAt,
                        Created = DateTime.Now
                    });
                    return false; // not outside TAT
                }

                // Evaluate whether this batch is outside TAT
                bool isOutside = false;
                if (setting != null)
                {
                    var threshold = TimeSpan.FromMinutes(setting.Hours * 60 + setting.Minutes);
                    var elapsed = endorsedAt - openCycle.CycleStart;
                    isOutside = elapsed > threshold;
                }

                // Close the current cycle
                openCycle.CycleEnd = endorsedAt;
                openCycle.ElapsedMinutes = (decimal)(endorsedAt - openCycle.CycleStart).TotalMinutes;
                openCycle.Result = isOutside ? "Outside" : "Within";
                openCycle.BatchNo = batchNo;
                unit.TatCycleLogs.Update(openCycle);

                // Open next cycle
                unit.TatCycleLogs.Add(new Tat_Cycle_Log
                {
                    SectionCode = sectionCode,
                    CycleStart = endorsedAt,
                    Created = DateTime.Now
                });

                return isOutside;
            }
            catch { throw; }
        }

        // ── Appeal ────────────────────────────────────────────────────────────

        public bool CanAppeal(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var setting = unit.TatSections.GetBySection(sectionCode);
                var openCycle = unit.TatCycleLogs.GetOpenCycle(sectionCode);

                // No open cycle = no active timer, nothing to appeal
                if (openCycle == null) return false;

                // No TAT setting = allow appeal freely
                if (setting == null) return true;

                if (setting.AppealWindow == "After")
                {
                    // Can only appeal after TAT is already breached
                    var threshold = TimeSpan.FromMinutes(setting.Hours * 60 + setting.Minutes);
                    var elapsed = DateTime.Now - openCycle.CycleStart;
                    return elapsed > threshold;
                }

                // "Before" = can appeal anytime while timer is running
                return true;
            }
            catch { throw; }
        }

        public void Appeal(string sectionCode, string userID, DateTime appealedAt)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var openCycle = unit.TatCycleLogs.GetOpenCycle(sectionCode);
                if (openCycle == null) return;

                // Close cycle as Appealed
                openCycle.CycleEnd = appealedAt;
                openCycle.ElapsedMinutes = (decimal)(appealedAt - openCycle.CycleStart).TotalMinutes;
                openCycle.Result = "Appealed";
                openCycle.AppealedBy = userID;
                unit.TatCycleLogs.Update(openCycle);

                // Open next cycle from appeal time
                unit.TatCycleLogs.Add(new Tat_Cycle_Log
                {
                    SectionCode = sectionCode,
                    CycleStart = appealedAt,
                    Created = DateTime.Now
                });
            }
            catch { throw; }
        }

        // ── Processing TAT ────────────────────────────────────────────────────────

        public Tat_Processing? GetProcessingTat()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.TatProcessing.Get(_branch_raw);
            }
            catch { throw; }
        }

        public void UpsertProcessingTat(int hours, int minutes, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // _branch here is the full connection string — we need the raw branch code
                // MasterService stores the raw branch code separately; use the overload
                var existing = unit.TatProcessing.Get(_branch_raw);
                if (existing == null)
                {
                    unit.TatProcessing.Add(new Tat_Processing
                    {
                        BranchCode = _branch_raw,
                        Hours = hours,
                        Minutes = minutes,
                        UpdatedBy = userID,
                        Updated = DateTime.Now
                    });
                }
                else
                {
                    existing.Hours = hours;
                    existing.Minutes = minutes;
                    existing.UpdatedBy = userID;
                    existing.Updated = DateTime.Now;
                    unit.TatProcessing.Update(existing);
                }

                context.SaveChanges();
            }
            catch { throw; }
        }

        public bool EvaluateProcessingTat(string batchNo, DateTime procReceived, DateTime completed)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var setting = unit.TatProcessing.Get(_branch_raw);

                // No setting configured — not flagged
                if (setting == null) return false;

                var threshold = TimeSpan.FromMinutes(setting.Hours * 60 + setting.Minutes);
                var elapsed = completed - procReceived;
                return elapsed > threshold;
            }
            catch { throw; }
        }
    }
}