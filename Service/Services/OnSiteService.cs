using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HCLAB;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class OnSiteService : IOnSiteService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public OnSiteService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        // ── Scan ───────────────────────────────────────────────────────────────
        public async Task<ScanOnSiteSpecimenResponse> ScanSpecimen(ScanOnSiteSpecimenRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var now = DateTime.Now;
                var oracleConn = HclabConnection.ConnectionString(_branch_raw);

                // 1. Check feature is enabled
                var settings = unit.OnSiteSettings.GetSettings();
                if (settings == null || !settings.IsEnabled)
                    throw new Exception("On-Site scanning is currently disabled.");

                // 2. Validate lab no. prefix (chars 3-4, zero-indexed [2..4] of labNo)
                if (request.SpecimenNo.Length < 10)
                    throw new Exception("Invalid specimen number.");

                var labNo = request.SpecimenNo.Substring(0, 10);
                var prefix = labNo.Substring(2, 2);

                var allowedPrefixes = unit.OnSiteAllowedLabNos.GetAllActive();
                if (!allowedPrefixes.Any(p => p.Prefix == prefix))
                    throw new Exception($"Specimen '{request.SpecimenNo}' is not allowed for On-Site scanning (prefix '{prefix}' not permitted).");

                // 3. Pull from HCLAB
                var rows = await HclabMaster.HCLABTransactions
                    .GetOnSiteSpecimen(oracleConn, request.SpecimenNo);

                if (rows == null || !rows.Any())
                    throw new Exception($"Specimen '{request.SpecimenNo}' was not found in HCLAB.");

                var first = rows.First();

                // 4. Validate test group is mapped to current section
                var testGroupCode = first.TestGroup;
                if (string.IsNullOrEmpty(testGroupCode))
                    throw new Exception($"Specimen '{request.SpecimenNo}' has no test group assigned in HCLAB.");

                var mapping = context.Section_TestGroup
                    .FirstOrDefault(m => m.Active
                                      && m.TestGroupCode == testGroupCode
                                      && m.SectionCode == request.SectionCode);

                if (mapping == null)
                    throw new Exception($"This section is not authorized to process specimens under test group '{testGroupCode}'.");

                // 5. Check for duplicate — already scanned by this section
                bool firstScan = false;
                var header = unit.OnSiteSectionHeaders
                    .GetBySpecimenAndSection(request.SpecimenNo, request.SectionCode);

                if (header == null)
                {
                    // First scan — create header + tests
                    firstScan = true;
                    header = new OnSite_Section_Header
                    {
                        SpecimenNo = first.SpecimenNo,
                        SectionCode = request.SectionCode,
                        TestGroupCode = testGroupCode,
                        SampleTypeCode = first.SampleTypeCode,
                        PatientName = first.PatientName,
                        PID = first.PID,
                        TransactionDate = first.TrxDate,
                        Status = "P",
                        ReceivedBy = request.UserID,
                        Received = now,
                        Created = now,
                        CreatedBy = request.UserID
                    };

                    unit.OnSiteSectionHeaders.Add(header);

                    foreach (var row in rows.Where(r => !string.IsNullOrEmpty(r.TestCode)))
                    {
                        unit.OnSiteSectionTests.Add(new OnSite_Section_Test
                        {
                            HeaderId = header.Id,
                            TestCode = row.TestCode,
                            TestName = row.TestName,
                            Status = "P",
                            Created = now
                        });
                    }
                }

                // ── Audit Logging ─────────────────────────────────────────────────────
                if (firstScan)
                {
                    try
                    {
                        using var auditMaster = new MasterService(_branch_raw);
                        auditMaster.Audit.Log(Audit_Log.SectionReceived(
                            header.SpecimenNo,
                            header.PatientName ?? string.Empty,
                            header.PID ?? string.Empty,
                            request.UserID,
                            request.SectionCode,
                            request.UserID));
                    }
                    catch (Exception auditEx)
                    {
                        Console.WriteLine($"[AUDIT] Logging failed for specimen {header.SpecimenNo}: {auditEx.Message}");
                    }
                }

                // 6. Return response — same shape as standard ScanSpecimenResponse
                var tests = unit.OnSiteSectionTests.GetByHeaderId(header.Id);

                return new ScanOnSiteSpecimenResponse
                {
                    HeaderId = header.Id,
                    SpecimenNo = header.SpecimenNo,
                    SectionCode = header.SectionCode,
                    TestGroupCode = header.TestGroupCode,
                    SampleTypeCode = header.SampleTypeCode,
                    SampleTypeName = unit.SampleTypes.GetByCode(header.SampleTypeCode)?.Name ?? header.SampleTypeCode,
                    PatientName = header.PatientName,
                    PID = header.PID,
                    TransactionDate = header.TransactionDate,
                    Status = header.Status,
                    FirstScan = firstScan,
                    ReceivedBy = header.ReceivedBy,
                    Received = header.Received,
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
                        HasRunningDay = context.Test_RunningDay.Any(r => r.TestCode == t.TestCode)
                    }).ToList()
                };
            }
            catch { throw; }
        }

        // ── Save Assignments ───────────────────────────────────────────────────
        // Mirrors RunnerService.SaveAssignments exactly, but targets OnSite tables.
        public void SaveAssignments(SaveAssignmentsRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();
                try
                {
                    var now = DateTime.Now;
                    var justRunTests = new List<(OnSite_Section_Test Test, string AssignedRMT)>();
                    var justScheduledTests = new List<(OnSite_Section_Test Test, string? PreviousTag)>();
                    var testIds = new HashSet<int>(request.Assignments.Select(a => a.TestId));
                    var today = DateOnly.FromDateTime(now);
                    var tests = context.OnSite_Section_Test
                        .Where(t => testIds.Contains(t.Id))
                        .ToList();

                    if (!tests.Any())
                        throw new Exception("No matching tests found.");

                    using var master = new MasterService(_branch_raw);

                    foreach (var assignment in request.Assignments)
                    {
                        var test = tests.FirstOrDefault(t => t.Id == assignment.TestId);
                        if (test == null) continue;

                        if (test.Status == "R" || test.Status == "X") continue;
                        var previousTag = test.ScheduleTag;
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
                                    justScheduledTests.Add((test, previousTag));
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
                                    justScheduledTests.Add((test, previousTag));
                                    break;

                                case "SRD":
                                    var nearestDate = master.TestRunningDay
                                        .GetNearestRunningDate(test.TestCode, today);

                                    if (nearestDate == null)
                                        throw new Exception(
                                            $"Test '{test.TestCode}' has no running day configured. " +
                                            $"Please set it up in Admin Settings → Running Days before using SRD.");

                                    test.ScheduleTag = "SRD";
                                    test.RunningDate = nearestDate;
                                    test.Status = "S";
                                    test.AssignedRMT = assignment.AssignedRMT;
                                    test.Assigned = null;
                                    justScheduledTests.Add((test, previousTag));
                                    break;
                            }
                        }
                        else
                        {
                            test.ScheduleTag = null;
                            test.RunningDate = null;

                            if (!string.IsNullOrEmpty(assignment.AssignedRMT))
                            {
                                test.Status = "R";
                                test.AssignedRMT = assignment.AssignedRMT;
                                test.Assigned = now;
                                test.RunAt = now;
                                justRunTests.Add((test, assignment.AssignedRMT));
                            }
                            else
                            {
                                test.Status = "P";
                                test.AssignedRMT = null;
                                test.Assigned = null;
                            }
                        }

                        context.OnSite_Section_Test.Update(test);
                    }

                    // Update remarks
                    if (request.SpecimenRemarks?.Any() == true)
                    {
                        var headerIds = request.SpecimenRemarks.Select(r => r.HeaderId).ToHashSet();
                        var headers = context.OnSite_Section_Header
                            .ToList()
                            .Where(h => headerIds.Contains(h.Id))
                            .ToList();

                        foreach (var remarkItem in request.SpecimenRemarks)
                        {
                            var header = headers.FirstOrDefault(h => h.Id == remarkItem.HeaderId);
                            if (header == null) continue;
                            header.Remarks = remarkItem.Remarks;
                            header.Updated = now;
                            header.UpdatedBy = request.UserID;
                            context.OnSite_Section_Header.Update(header);
                        }
                    }

                    context.SaveChanges();

                    // Re-derive header status from child tests
                    var affectedHeaderIds = tests.Select(t => t.HeaderId).Distinct().ToList();
                    foreach (var headerId in affectedHeaderIds)
                    {
                        var allTests = context.OnSite_Section_Test
                            .Where(t => t.HeaderId == headerId)
                            .ToList();

                        var header = context.OnSite_Section_Header.Find(headerId);
                        if (header == null) continue;

                        if (allTests.All(t => t.Status == "X"))
                            header.Status = "C";
                        else if (allTests.Any(t => t.Status == "R" || t.Status == "S" || t.Status == "X"))
                            header.Status = "S";
                        else
                            header.Status = "P";

                        header.Updated = now;
                        header.UpdatedBy = request.UserID;
                        context.OnSite_Section_Header.Update(header);
                    }

                    context.SaveChanges();
                    tx.Commit();

                    // ── Audit Logging ──────────────────────────────────────────────────────
                    try
                    {
                        using var auditMaster = new MasterService(_branch_raw);

                        // ── TEST_RUN ───────────────────────────────────────────────────────
                        var runGroups = justRunTests
                            .GroupBy(x => x.Test.HeaderId)
                            .ToList();

                        foreach (var group in runGroups)
                        {
                            var header = context.OnSite_Section_Header.Find(group.Key);
                            if (header == null) continue;

                            var testNames = string.Join(",", group.Select(x => $"{x.Test.TestCode}:{x.Test.TestName}"));
                            var rmtUserID = group.First().AssignedRMT;

                            auditMaster.Audit.Log(Audit_Log.TestRun(
                                header.SpecimenNo,
                                header.PatientName ?? string.Empty,
                                header.PID ?? string.Empty,
                                header.SectionCode,
                                testNames,
                                rmtUserID,
                                request.UserID,
                                now));
                        }

                        // ── SPECIMEN_STORED / TEST_SCHEDULED / TEST_RESCHEDULED ───────────
                        var scheduledGroups = justScheduledTests
                            .GroupBy(x => x.Test.HeaderId)
                            .ToList();

                        foreach (var group in scheduledGroups)
                        {
                            var header = context.OnSite_Section_Header.Find(group.Key);
                            if (header == null) continue;

                            var patientName = header.PatientName ?? string.Empty;
                            var pid = header.PID ?? string.Empty;

                            auditMaster.Audit.Log(Audit_Log.SpecimenStored(
                                header.SpecimenNo,
                                patientName,
                                pid,
                                header.SectionCode,
                                request.UserID));

                            foreach (var item in group)
                            {
                                var runningDate = item.Test.RunningDate.HasValue
                                    ? item.Test.RunningDate.Value.ToDateTime(TimeOnly.MinValue)
                                    : DateTime.Today;

                                var testEntry = $"{item.Test.TestCode}:{item.Test.TestName}";

                                if (item.PreviousTag == null)
                                {
                                    auditMaster.Audit.Log(Audit_Log.TestScheduled(
                                        header.SpecimenNo,
                                        patientName,
                                        pid,
                                        header.SectionCode,
                                        testEntry,
                                        item.Test.ScheduleTag!,
                                        runningDate,
                                        request.UserID));
                                }
                                else if (item.PreviousTag != item.Test.ScheduleTag)
                                {
                                    auditMaster.Audit.Log(Audit_Log.TestRescheduled(
                                        header.SpecimenNo,
                                        patientName,
                                        pid,
                                        header.SectionCode,
                                        testEntry,
                                        item.PreviousTag,
                                        item.Test.ScheduleTag!,
                                        runningDate,
                                        request.UserID));
                                }
                            }
                        }
                    }
                    catch (Exception auditEx)
                    {
                        Console.WriteLine($"[AUDIT] Logging failed in OnSite SaveAssignments: {auditEx.Message}");
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }

        // ── Pending ────────────────────────────────────────────────────────────
        public List<PendingSpecimenItem> GetPendingSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode
                             && (h.Status == "P" || h.Status == "S"))
                    .ToList();

                if (!headers.Any()) return new List<PendingSpecimenItem>();

                var headerIds = headers.Select(h => h.Id).ToList();

                var allTests = context.OnSite_Section_Test
                    .Where(t => headerIds.Contains(t.HeaderId)
                             && (t.Status == "P" || t.Status == "R"))
                    .ToList();

                return headers
                    .Where(h => allTests.Any(t => t.HeaderId == h.Id))
                    .Select(h => new PendingSpecimenItem
                    {
                        Id = h.Id,
                        SpecimenNo = h.SpecimenNo,
                        SectionCode = h.SectionCode,
                        TestGroupCode = h.TestGroupCode,
                        SampleTypeCode = h.SampleTypeCode,
                        SampleTypeName = h.SampleTypeCode,
                        Status = h.Status,
                        ReceivedBy = h.ReceivedBy,
                        Received = h.Received,
                        Remarks = h.Remarks,
                        PatientName = h.PatientName,
                        PatientID = h.PID,
                    })
                    .OrderBy(s => s.Received)
                    .ToList();
            }
            catch { throw; }
        }

        public List<OnSite_Section_Test> GetTestsByHeaderId(int headerId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.OnSite_Section_Test
                    .Where(t => t.HeaderId == headerId)
                    .OrderBy(t => t.TestCode)
                    .ToList();
            }
            catch { throw; }
        }

        // ── Scheduled ──────────────────────────────────────────────────────────
        public List<ScheduledSpecimenItem> GetScheduledSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode && h.Status == "S")
                    .ToList();

                if (!headers.Any()) return new List<ScheduledSpecimenItem>();

                var headerIds = headers.Select(h => h.Id).ToList();

                var savedTests = context.OnSite_Section_Test
                    .Where(t => headerIds.Contains(t.HeaderId) && t.Status == "S")
                    .ToList();

                if (!savedTests.Any()) return new List<ScheduledSpecimenItem>();

                var relevantHeaderIds = savedTests.Select(t => t.HeaderId).Distinct().ToHashSet();

                return headers
                    .Where(h => relevantHeaderIds.Contains(h.Id))
                    .Select(h =>
                    {
                        var tests = savedTests.Where(t => t.HeaderId == h.Id).ToList();
                        return new ScheduledSpecimenItem
                        {
                            HeaderId = h.Id,
                            SpecimenNo = h.SpecimenNo,
                            SectionCode = h.SectionCode,
                            SampleTypeCode = h.SampleTypeCode,
                            SampleTypeName = null,
                            PatientName = h.PatientName,
                            PatientID = h.PID,
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

        // ── Running ────────────────────────────────────────────────────────────
        public List<RunningSpecimenItem> GetRunningSpecimens(string sectionCode, string? userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();

                if (!headers.Any()) return new List<RunningSpecimenItem>();

                var headerIds = headers.Select(h => h.Id).ToList();

                var runningTests = context.OnSite_Section_Test
                    .Where(t => headerIds.Contains(t.HeaderId)
                             && t.Status == "R"
                             && (userID == null || t.AssignedRMT == userID))
                    .ToList();

                if (!runningTests.Any()) return new List<RunningSpecimenItem>();

                var relevantHeaderIds = runningTests.Select(t => t.HeaderId).Distinct().ToHashSet();

                return headers
                    .Where(h => relevantHeaderIds.Contains(h.Id))
                    .Select(h =>
                    {
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
                            SampleTypeName = null,
                            PatientName = h.PatientName,
                            PatientID = h.PID,
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

        // ── Middleware ─────────────────────────────────────────────────────────────

        public List<OnSite_Section_Test> GetDueScheduledTests(DateOnly today)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.OnSite_Section_Test
                    .Where(t => t.Status == "S"
                             && t.RunningDate != null
                             && t.RunningDate <= today
                             && (t.ScheduleTag == "END" || t.ScheduleTag == "CRD" || t.ScheduleTag == "SRD"))
                    .ToList();
            }
            catch { throw; }
        }

        public void ReleaseScheduledHeaders(List<int> headerIds)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var today = DateOnly.FromDateTime(DateTime.Today);

                foreach (var id in headerIds)
                {
                    var header = unit.OnSiteSectionHeaders.Get(id);
                    if (header == null || header.Status == "P") continue;

                    // Only flip to P if ALL scheduled tests are due today or past.
                    var allTests = context.OnSite_Section_Test
                        .Where(t => t.HeaderId == id && t.Status == "S")
                        .ToList();

                    var hasFutureTests = allTests
                        .Any(t => t.RunningDate.HasValue && t.RunningDate.Value > today);

                    if (hasFutureTests) continue;

                    header.Status = "P";
                    header.Updated = DateTime.Now;
                    unit.OnSiteSectionHeaders.Update(header);
                }
            }
            catch { throw; }
        }

        public List<OnSite_Section_Test> GetAllRunningTests()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.OnSite_Section_Test
                    .Where(t => t.Status == "R")
                    .ToList();
            }
            catch { throw; }
        }

        public OnSite_Section_Header? GetHeaderById(int id)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.OnSite_Section_Header.Find(id);
            }
            catch { throw; }
        }

        public void MarkTestReleased(int testId, string? releasedBy, DateTime? releasedOn)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                var test = unit.OnSiteSectionTests.Get(testId);
                if (test == null) return;
                test.Status = "X";
                test.Updated = DateTime.Now;
                test.ReleasedBy = releasedBy;
                test.ReleasedOn = releasedOn;
                unit.OnSiteSectionTests.Update(test);
            }
            catch { throw; }
        }

        public bool TryCompleteHeader(int headerId)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var allTests = context.OnSite_Section_Test
                    .Where(t => t.HeaderId == headerId)
                    .ToList();

                var header = context.OnSite_Section_Header.Find(headerId);
                if (header == null || header.Status == "C") return false;

                if (allTests.All(t => t.Status == "X"))
                    header.Status = "C";
                else if (allTests.Any(t => t.Status == "R" || t.Status == "S" || t.Status == "X"))
                    header.Status = "S";
                else
                    header.Status = "P";

                header.Updated = DateTime.Now;
                context.SaveChanges();

                return true;
            }
            catch { throw; }
        }

        // ── Cancel specimen (runner/section side) ──────────────────────────
        // TL / Admin only (enforced at controller level).
        // Sets OnSite header Status = "X" and cascades to all non-released child tests.
        public void CancelSpecimen(CancelSectionSpecimenRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                try
                {
                    var now = DateTime.Now;

                    // 1. Find the header
                    var header = context.OnSite_Section_Header
                        .FirstOrDefault(h => h.Id == request.HeaderId
                                          && h.SectionCode == request.SectionCode)
                        ?? throw new Exception(
                            $"On-Site specimen '{request.SpecimenNo}' not found in section '{request.SectionCode}'.");

                    if (header.Status == "X")
                        throw new Exception($"Specimen '{request.SpecimenNo}' is already cancelled.");

                    if (header.Status == "C")
                        throw new Exception($"Specimen '{request.SpecimenNo}' is already completed and cannot be cancelled.");

                    // 2. Cancel the header
                    header.Status = "X";
                    header.Updated = now;
                    context.OnSite_Section_Header.Update(header);

                    // 3. Cancel all non-released child tests
                    var tests = context.OnSite_Section_Test
                        .Where(t => t.HeaderId == header.Id && t.Status != "X")
                        .ToList();

                    foreach (var test in tests)
                    {
                        test.Status = "X";
                        test.Updated = now;
                        context.OnSite_Section_Test.Update(test);
                    }

                    context.SaveChanges();
                    tx.Commit();

                    // 4. Audit — fire-and-forget
                    try
                    {
                        using var auditMaster = new MasterService(_branch_raw);
                        auditMaster.Audit.Log(Audit_Log.SpecimenCancelledSection(
                            request.SpecimenNo,
                            request.SectionCode,
                            header.PatientName ?? string.Empty,
                            header.PID ?? string.Empty,
                            request.Reason,
                            request.UserID));
                    }
                    catch (Exception auditEx)
                    {
                        Console.WriteLine($"[AUDIT] OnSite CancelSpecimen logging failed for {request.SpecimenNo}: {auditEx.Message}");
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }

        // ── Abort a single running On-Site test back to pending ────────────
        // Available to all users.
        public void AbortRunningTest(AbortRunningTestRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var tx = context.Database.BeginTransaction();

                try
                {
                    var now = DateTime.Now;

                    // 1. Find the test
                    var test = context.OnSite_Section_Test
                        .FirstOrDefault(t => t.Id == request.TestId)
                        ?? throw new Exception($"On-Site test ID {request.TestId} not found.");

                    if (test.Status != "R")
                        throw new Exception($"Only running tests can be aborted. Current status: {test.Status}.");

                    // 2. Reset the test
                    test.Status = "P";
                    test.AssignedRMT = null;
                    test.Assigned = null;
                    test.RunAt = null;
                    test.Updated = now;
                    context.OnSite_Section_Test.Update(test);

                    // 3. Re-derive header status from all sibling tests
                    var header = context.OnSite_Section_Header
                        .FirstOrDefault(h => h.Id == request.HeaderId)
                        ?? throw new Exception($"On-Site header ID {request.HeaderId} not found.");

                    var allSiblingTests = context.OnSite_Section_Test
                        .Where(t => t.HeaderId == request.HeaderId)
                        .ToList();

                    // Apply the aborted test's new status in-memory before deriving
                    var sibling = allSiblingTests.FirstOrDefault(t => t.Id == request.TestId);
                    if (sibling != null) sibling.Status = "P";

                    string newHeaderStatus;
                    if (allSiblingTests.All(t => t.Status == "X"))
                        newHeaderStatus = "C";
                    else
                        newHeaderStatus = "P";

                    if (header.Status != newHeaderStatus)
                    {
                        header.Status = newHeaderStatus;
                        header.Updated = now;
                        context.OnSite_Section_Header.Update(header);
                    }

                    context.SaveChanges();
                    tx.Commit();

                    // 4. Audit — fire-and-forget
                    // OnSite_Section_Header carries PatientName/PID directly — no extra lookup needed.
                    try
                    {
                        using var auditMaster = new MasterService(_branch_raw);
                        auditMaster.Audit.Log(Audit_Log.TestAborted(
                            request.SpecimenNo,
                            header.PatientName ?? string.Empty,
                            header.PID ?? string.Empty,
                            request.SectionCode,
                            test.TestCode,
                            test.TestName,
                            request.Reason,
                            request.UserID));
                    }
                    catch (Exception auditEx)
                    {
                        Console.WriteLine($"[AUDIT] OnSite AbortRunningTest logging failed for {request.SpecimenNo} / test {request.TestId}: {auditEx.Message}");
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            catch { throw; }
        }
    }
}