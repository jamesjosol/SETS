using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;
using static System.Collections.Specialized.BitVector32;

namespace Service.Services
{
    public class RunnerService : IRunnerService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public RunnerService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── Scan specimen ──────────────────────────────────────────────────
        // Called when user scans a barcode on the Assign RMT page.
        // - Finds the header for this specimen in this section
        // - Stamps ReceivedBy/Received on the header if this is the first scan
        // - Returns the header info + all child tests
        public ScanSpecimenResponse ScanSpecimen(ScanSpecimenRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                try
                {
                    var now = DateTime.Now;

                    // 1. Find the header — must exist (routed by processing)
                    var header = context.Specimen_Section_Header
                        .FirstOrDefault(h => h.SpecimenNo == request.SpecimenNo
                                          && h.SectionCode == request.SectionCode)
                        ?? throw new Exception(
                            $"Specimen '{request.SpecimenNo}' has not been routed to this section.");

                    if (!header.IsHclabRouted)
                        throw new Exception($"Specimen '{request.SpecimenNo}' has not been routed in HCLAB yet.");

                    // 2. Stamp received — ONLY if first scan
                    bool firstScan = header.ReceivedBy == null;
                    if (firstScan)
                    {
                        header.ReceivedBy = request.UserID;
                        header.Received = now;
                        header.Updated = now;
                        header.UpdatedBy = request.UserID;
                        context.Specimen_Section_Header.Update(header);
                        context.SaveChanges();
                    }

                    // 3. Load child tests
                    var tests = context.Specimen_Section_Test
                        .Where(t => t.HeaderId == header.Id)
                        .OrderBy(t => t.TestCode)
                        .ToList();

                    tx.Commit();

                    var testCodes = tests.Select(t => t.TestCode).ToList();
                    var testCodesWithRunningDay = context.Test_RunningDay
                        .ToList()
                        .Where(r => testCodes.Contains(r.TestCode))
                        .Select(r => r.TestCode)
                        .ToHashSet();


                    var sampleTypeName = context.Sample_Type
                        .FirstOrDefault(s => s.Code == header.SampleTypeCode)?.Name;

                    return new ScanSpecimenResponse
                    {
                        HeaderId = header.Id,
                        SpecimenNo = header.SpecimenNo,
                        SectionCode = header.SectionCode,
                        TestGroupCode = header.TestGroupCode,
                        SampleTypeCode = header.SampleTypeCode,
                        SampleTypeName = sampleTypeName ?? header.SampleTypeCode,
                        Status = header.Status,
                        FirstScan = firstScan,
                        Received = header.Received,
                        ReceivedBy = header.ReceivedBy,
                        Remarks = header.Remarks,
                        Tests = tests.Select(t => new ScanSpecimenTestItem
                        {
                            Id = t.Id,
                            TestCode = t.TestCode,
                            TestName = t.TestName,
                            Status = t.Status,
                            ScheduleTag = t.ScheduleTag,
                            RunningDate = t.RunningDate,
                            AssignedRMT = t.AssignedRMT,
                            Assigned = t.Assigned,
                            RunAt = t.RunAt,
                            HasRunningDay = testCodesWithRunningDay.Contains(t.TestCode)
                        }).ToList()
                    };
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }

        // ── Save assignments ───────────────────────────────────────────────
        // Updates each test row:
        // - AssignedRMT set + no tag      → Status = R (Running)
        // - ScheduleTag set               → Status = S (Saved)
        // - Neither                       → Status = P (Pending)
        // Then re-derives header status from all its child tests.

        public void SaveAssignments(SaveAssignmentsRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();
                try
                {
                    var now = DateTime.Now;
                    var today = DateOnly.FromDateTime(now);
                    var testIds = new HashSet<int>(request.Assignments.Select(a => a.TestId));

                    // Load all tests in memory first — avoids EF Core CTE syntax error with Contains()
                    var tests = context.Specimen_Section_Test
                        .ToList()
                        .Where(t => testIds.Contains(t.Id))
                        .ToList();

                    if (!tests.Any())
                        throw new Exception("No matching tests found.");

                    // ── Instantiate running day service once for the whole batch ──────
                    using var master = new MasterService(_branch_raw);

                    foreach (var assignment in request.Assignments)
                    {
                        var test = tests.FirstOrDefault(t => t.Id == assignment.TestId);
                        if (test == null) continue;

                        // Skip tests that are already Running or Released
                        if (test.Status == "R" || test.Status == "X") continue;

                        test.UpdatedBy = request.UserID;
                        test.Updated = now;

                        if (!string.IsNullOrEmpty(assignment.ScheduleTag))
                        {
                            switch (assignment.ScheduleTag)
                            {
                                case "END":
                                    test.ScheduleTag = "END";
                                    test.RunningDate = DateOnly.FromDateTime(now.AddDays(1));
                                    test.Status = "S";
                                    test.AssignedRMT = assignment.AssignedRMT;
                                    test.Assigned = null;
                                    break;

                                case "CRD":
                                    if (assignment.RunningDate == null)
                                        throw new Exception(
                                            $"Test '{test.TestCode}': A running date is required for CRD.");

                                    if (assignment.RunningDate <= DateOnly.FromDateTime(now))
                                        throw new Exception(
                                            $"Test '{test.TestCode}': CRD running date must be a future date.");

                                    test.ScheduleTag = "CRD";
                                    test.RunningDate = assignment.RunningDate;
                                    test.Status = "S";
                                    test.AssignedRMT = assignment.AssignedRMT;
                                    test.Assigned = null;
                                    break;

                                // ── SRD — look up nearest configured running day ──────
                                case "SRD":
                                    var nearestDate = master.TestRunningDay
                                        .GetNearestRunningDate(test.TestCode, today);

                                    if (nearestDate == null)
                                        throw new Exception(
                                            $"Test '{test.TestCode}' has no running day configured. " +
                                            $"Please set it up in Admin Settings → Running Days before using SRD.");

                                    test.ScheduleTag = "SRD";
                                    test.RunningDate = nearestDate;   // nearest day >= today
                                    test.Status = "S";
                                    test.AssignedRMT = assignment.AssignedRMT;
                                    test.Assigned = null;
                                    break;
                            }
                        }
                        else
                        {
                            // Today — no tag, no date
                            test.ScheduleTag = null;
                            test.RunningDate = null;

                            if (!string.IsNullOrEmpty(assignment.AssignedRMT))
                            {
                                test.Status = "R";
                                test.AssignedRMT = assignment.AssignedRMT;
                                test.Assigned = now;
                                test.RunAt = DateTime.Now;
                            }
                            else
                            {
                                test.Status = "P";
                                test.AssignedRMT = null;
                                test.Assigned = null;
                            }
                        }

                        context.Specimen_Section_Test.Update(test);
                    }

                    // Update specimen remarks
                    if (request.SpecimenRemarks?.Any() == true)
                    {
                        var headerIds = request.SpecimenRemarks.Select(r => r.HeaderId).ToHashSet();
                        var _headers = context.Specimen_Section_Header
                            .ToList()
                            .Where(h => headerIds.Contains(h.Id))
                            .ToList();


                        foreach (var remarkItem in request.SpecimenRemarks)
                        {
                            var header = _headers.FirstOrDefault(h => h.Id == remarkItem.HeaderId);
                            if (header == null) continue;
                            header.Remarks = remarkItem.Remarks;
                            header.UpdatedBy = request.UserID;
                            header.Updated = now;
                            context.Specimen_Section_Header.Update(header);
                        }
                    }

                    var affectedHeaderIds = tests
                        .Where(t => request.Assignments.Any(a => a.TestId == t.Id))
                        .Select(t => t.HeaderId)
                        .Distinct()
                        .ToList();

                    var allTestsForHeaders = context.Specimen_Section_Test
                        .ToList()
                        .Where(t => affectedHeaderIds.Contains(t.HeaderId))
                        .ToList();

                    var headers = context.Specimen_Section_Header
                        .ToList()
                        .Where(h => affectedHeaderIds.Contains(h.Id))
                        .ToList();

                    foreach (var header in headers)
                    {
                        var headerTests = allTestsForHeaders.Where(t => t.HeaderId == header.Id).ToList();

                        string newStatus;
                        if (headerTests.All(t => t.Status == "X"))
                            newStatus = "C";
                        else if (headerTests.Any(t => t.Status == "R" || t.Status == "S" || t.Status == "X"))
                            newStatus = "S";
                        else
                            newStatus = "P";

                        if (header.Status != newStatus)
                        {
                            header.Status = newStatus;
                            header.UpdatedBy = request.UserID;
                            header.Updated = now;
                            context.Specimen_Section_Header.Update(header);
                        }
                    }

                    context.SaveChanges();
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }
        // ── Get pending specimens ──────────────────────────────────────────

        public List<PendingSpecimenItem> GetPendingSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.Specimen_Section_Header
                      .Where(h => h.SectionCode == sectionCode
                               && h.Status == "P"
                               && h.IsHclabRouted)          // ← add this
                      .OrderByDescending(h => h.Routed)
                      .ToList();

                if (!headers.Any()) return new List<PendingSpecimenItem>();

                // Pull patient info and sample type name from Batch_Specimen (all local MSSQL)
                var specimenNos = headers.Select(h => h.SpecimenNo).ToList();
                var batchSpecimens = context.Batch_Specimen
                    .ToList()
                    .Where(b => specimenNos.Contains(b.SpecimenNo))
                    .ToDictionary(b => b.SpecimenNo, b => b);

                return headers.Select(h =>
                {
                    batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                    return new PendingSpecimenItem
                    {
                        Id = h.Id,
                        SpecimenNo = h.SpecimenNo,
                        SectionCode = h.SectionCode,
                        TestGroupCode = h.TestGroupCode,
                        SampleTypeCode = h.SampleTypeCode,
                        SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
                        Status = h.Status,
                        RoutedBy = h.RoutedBy,
                        Routed = h.Routed,
                        ReceivedBy = h.ReceivedBy,
                        Received = h.Received,
                        Remarks = h.Remarks,
                        PatientName = bs?.PatientName,
                        PatientID = bs?.PID,
                    };
                }).ToList();
            }
            catch { throw; }
        }

        // ── Get scheduled specimens ────────────────────────────────────────────
        public List<ScheduledSpecimenItem> GetScheduledSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                // Get all headers for this section that have at least one saved test
                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();

                if (!headers.Any()) return new List<ScheduledSpecimenItem>();

                var headerIds = headers.Select(h => h.Id).ToList();

                // Get all saved tests under those headers
                var allTests = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => headerIds.Contains(t.HeaderId) && t.Status == "S")
                    .ToList();

                if (!allTests.Any()) return new List<ScheduledSpecimenItem>();

                // Only keep headers that actually have saved tests
                var relevantHeaderIds = allTests.Select(t => t.HeaderId).Distinct().ToHashSet();
                var relevantHeaders = headers.Where(h => relevantHeaderIds.Contains(h.Id)).ToList();

                // Join Batch_Specimen for patient info and sample type name
                var specimenNos = relevantHeaders.Select(h => h.SpecimenNo).ToList();
                var batchSpecimens = context.Batch_Specimen
                    .ToList()
                    .Where(b => specimenNos.Contains(b.SpecimenNo))
                    .ToDictionary(b => b.SpecimenNo, b => b);

                return relevantHeaders.Select(h =>
                {
                    batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                    var tests = allTests.Where(t => t.HeaderId == h.Id)
                        .OrderBy(t => t.RunningDate)
                        .ThenBy(t => t.TestCode)
                        .ToList();

                    return new ScheduledSpecimenItem
                    {
                        HeaderId = h.Id,
                        SpecimenNo = h.SpecimenNo,
                        SectionCode = h.SectionCode,
                        SampleTypeCode = h.SampleTypeCode,
                        SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
                        PatientName = bs?.PatientName,
                        PatientID = bs?.PID,
                        Remarks = h.Remarks,
                        ReceivedBy = h.ReceivedBy,
                        Received = h.Received,
                        Tests = tests.Select(t => new ScheduledTestItem
                        {
                            Id = t.Id,
                            TestCode = t.TestCode,
                            TestName = t.TestName,
                            Status = t.Status,
                            ScheduleTag = t.ScheduleTag,
                            RunningDate = t.RunningDate,
                            AssignedRMT = t.AssignedRMT,
                        }).ToList()
                    };
                })
                .OrderBy(s => s.Tests.Min(t => t.RunningDate ?? DateOnly.MaxValue))
                .ToList();
            }
            catch { throw; }
        }

        public List<RunningSpecimenItem> GetRunningSpecimens(string sectionCode, string? userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();

                if (!headers.Any()) return new List<RunningSpecimenItem>();

                var headerIds = headers.Select(h => h.Id).ToList();

                // Only tests that are Running and assigned to this user
                var runningTests = context.Specimen_Section_Test
             .ToList()
             .Where(t => headerIds.Contains(t.HeaderId)
                      && t.Status == "R"
                      && (userID == null || t.AssignedRMT == userID))  // ← null = all users
             .ToList();

                if (!runningTests.Any()) return new List<RunningSpecimenItem>();

                var relevantHeaderIds = runningTests.Select(t => t.HeaderId).Distinct().ToHashSet();
                var relevantHeaders = headers.Where(h => relevantHeaderIds.Contains(h.Id)).ToList();

                // Join Batch_Specimen for patient info and sample type name
                var specimenNos = relevantHeaders.Select(h => h.SpecimenNo).ToList();
                var batchSpecimens = context.Batch_Specimen
                    .ToList()
                    .Where(b => specimenNos.Contains(b.SpecimenNo))
                    .ToDictionary(b => b.SpecimenNo, b => b);

                return relevantHeaders.Select(h =>
                {
                    batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                    var tests = runningTests
                        .Where(t => t.HeaderId == h.Id)
                        .OrderBy(t => t.RunAt)
                        .ThenBy(t => t.TestCode)
                        .ToList();

                    return new RunningSpecimenItem
                    {
                        HeaderId = h.Id,
                        SpecimenNo = h.SpecimenNo,
                        SectionCode = h.SectionCode,
                        SampleTypeCode = h.SampleTypeCode,
                        SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
                        PatientName = bs?.PatientName,
                        PatientID = bs?.PID,
                        Remarks = h.Remarks,
                        Received = h.Received,
                        Tests = tests.Select(t => new RunningTestItem
                        {
                            Id = t.Id,
                            TestCode = t.TestCode,
                            TestName = t.TestName,
                            AssignedRMT = t.AssignedRMT,
                            Assigned = t.Assigned,
                            RunAt = t.RunAt,
                        }).ToList()
                    };
                })
                .OrderBy(s => s.Tests.Min(t => t.RunAt ?? DateTime.MaxValue))
                .ToList();
            }
            catch { throw; }
        }
        public RunnerDashboardSummary GetDashboardSummary(string sectionCode, string userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode && h.IsHclabRouted)
                    .ToList();

                var headerIds = headers.Select(h => h.Id).ToList();

                var allTests = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => headerIds.Contains(t.HeaderId))
                    .ToList();

                var today = DateOnly.FromDateTime(DateTime.Today);

                return new RunnerDashboardSummary
                {
                    // Headers with at least one pending test
                    Pending = headers.Count(h =>
                        h.Status == "P" || h.Status == "S" || h.Status == "R"),

                    // Tests currently saved (scheduled)
                    Scheduled = allTests.Count(t => t.Status == "S"),

                    // Tests currently running assigned to this user
                    Running = allTests.Count(t => t.Status == "R" && t.AssignedRMT == userID),

                    // Headers completed today (all tests released, updated today)
                    CompletedToday = headers.Count(h =>
                        h.Status == "C" &&
                        h.Updated.HasValue &&
                        DateOnly.FromDateTime(h.Updated.Value) == today),
                };
            }
            catch { throw; }
        }

        public List<LabSectionSummary> GetAllSectionsSummary()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var today = DateOnly.FromDateTime(DateTime.Today);

                return sections.Select(s =>
                {
                    var headers = context.Specimen_Section_Header
                        .Where(h => h.SectionCode == s.Code && h.IsHclabRouted)
                        .ToList();

                    var headerIds = headers.Select(h => h.Id).ToList();

                    var allTests = context.Specimen_Section_Test
                        .ToList()
                        .Where(t => headerIds.Contains(t.HeaderId))
                        .ToList();

                    return new LabSectionSummary
                    {
                        SectionCode = s.Code,
                        SectionName = s.Name,
                        Pending = headers.Count(h =>
                                             h.Status == "P" || h.Status == "S" || h.Status == "R"),
                        Scheduled = allTests.Count(t => t.Status == "S"),
                        Running = allTests.Count(t => t.Status == "R"),
                        CompletedToday = headers.Count(h =>
                                             h.Status == "C" &&
                                             h.Updated.HasValue &&
                                             DateOnly.FromDateTime(h.Updated.Value) == today),
                    };
                }).ToList();
            }
            catch { throw; }
        }

        public List<SectionRunningGroup> GetAllSectionsRunning()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var result = new List<SectionRunningGroup>();

                foreach (var section in sections)
                {
                    var specimens = GetRunningSpecimens(section.Code, null); // null = all users
                    if (!specimens.Any()) continue;

                    result.Add(new SectionRunningGroup
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Specimens = specimens,
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public List<SectionPendingGroup> GetAllSectionsRecentlyRouted()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var result = new List<SectionPendingGroup>();

                foreach (var section in sections)
                {
                    var all = GetPendingSpecimens(section.Code);
                    var unReceived = all.Where(s => s.ReceivedBy == null).ToList();
                    if (!unReceived.Any()) continue;

                    result.Add(new SectionPendingGroup
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Specimens = unReceived,
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public List<SectionScheduledGroup> GetAllSectionsDueToday()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var todayStr = DateOnly.FromDateTime(DateTime.Today);
                var result = new List<SectionScheduledGroup>();

                foreach (var section in sections)
                {
                    var scheduled = GetScheduledSpecimens(section.Code);
                    var due = scheduled.Where(s =>
                        s.Tests.Any(t =>
                            (t.ScheduleTag == "END" || t.ScheduleTag == "CRD" || t.ScheduleTag == "SRD") &&
                            t.RunningDate == todayStr
                        )).ToList();

                    if (!due.Any()) continue;

                    result.Add(new SectionScheduledGroup
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Specimens = due,
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public List<SectionPendingGroup> GetAllSectionsCompletedToday()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var result = new List<SectionPendingGroup>();

                foreach (var section in sections)
                {
                    var completed = GetCompletedToday(section.Code);  // ← was GetPendingSpecimens
                    if (!completed.Any()) continue;

                    result.Add(new SectionPendingGroup
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Specimens = completed,
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public List<PendingSpecimenItem> GetCompletedToday(string sectionCode)
        {
            using var context = _factory.CreateContext(_branch);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var headers = context.Specimen_Section_Header
                .Where(h => h.SectionCode == sectionCode
                         && h.Status == "C"
                         && h.Updated.HasValue
                         && DateOnly.FromDateTime(h.Updated.Value) == today)
                .OrderByDescending(h => h.Updated)
                .ToList();

            if (!headers.Any()) return new List<PendingSpecimenItem>();

            var specimenNos = headers.Select(h => h.SpecimenNo).ToList();
            var batchSpecimens = context.Batch_Specimen
                .ToList()
                .Where(b => specimenNos.Contains(b.SpecimenNo))
                .ToDictionary(b => b.SpecimenNo, b => b);

            return headers.Select(h =>
            {
                batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                return new PendingSpecimenItem
                {
                    Id = h.Id,
                    SpecimenNo = h.SpecimenNo,
                    SectionCode = h.SectionCode,
                    TestGroupCode = h.TestGroupCode,
                    SampleTypeCode = h.SampleTypeCode,
                    SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
                    Status = h.Status,
                    RoutedBy = h.RoutedBy,
                    Routed = h.Routed,
                    ReceivedBy = h.ReceivedBy,
                    Received = h.Received,
                    Remarks = h.Remarks,
                    PatientName = bs?.PatientName,
                    PatientID = bs?.PID,
                };
            }).ToList();
        }

        // for admin
        public List<SectionPendingGroup> GetAllSectionsPending()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var result = new List<SectionPendingGroup>();

                foreach (var section in sections)
                {
                    var specimens = GetPendingSpecimens(section.Code);
                    if (!specimens.Any()) continue;

                    result.Add(new SectionPendingGroup
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Specimens = specimens,
                    });
                }

                return result;
            }
            catch { throw; }
        }

        public List<SectionScheduledGroup> GetAllSectionsScheduled()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var result = new List<SectionScheduledGroup>();

                foreach (var section in sections)
                {
                    var specimens = GetScheduledSpecimens(section.Code);
                    if (!specimens.Any()) continue;

                    result.Add(new SectionScheduledGroup
                    {
                        SectionCode = section.Code,
                        SectionName = section.Name,
                        Specimens = specimens,
                    });
                }

                return result;
            }
            catch { throw; }
        }

        // ── Get tests by header ────────────────────────────────────────────
        public List<Specimen_Section_Test> GetTestsByHeader(int headerId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.Specimen_Section_Test
                    .Where(t => t.HeaderId == headerId)
                    .OrderBy(t => t.TestCode)
                    .ToList();
            }
            catch { throw; }
        }
    }
}