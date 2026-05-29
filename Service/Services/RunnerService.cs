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

                    var receivingRecord = context.Batch_Specimen_Receiving
                        .FirstOrDefault(r => r.SpecimenNo == request.SpecimenNo);

                    string? specimenAlert = receivingRecord?.SpecimenAlert;
                    string? specimenAlertSetBy = receivingRecord?.SpecimenAlertSetBy;
                    DateTime? specimenAlertSetAt = receivingRecord?.SpecimenAlertSetAt;

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

                    // ── Audit Logging ─────────────────────────────────────────────────────
                    Batch_Specimen temp = null;
                    if (firstScan)
                    {
                        try
                        {
                            using var master = new MasterService(_branch_raw);
                            temp = context.Batch_Specimen.FirstOrDefault(a => a.SpecimenNo == header.SpecimenNo);
                            string? loc = null;
                            if (temp != null)
                            {
                                loc = context.Batch_Header.FirstOrDefault(a => a.BatchNo == temp.BatchNo)?.ProcDestination;
                            }
                            master.Audit.Log(Audit_Log.SectionReceived(
                                request.SpecimenNo,
                                temp?.PatientName ?? "",
                                temp?.PID ?? "",
                                loc ?? "SYSTEM",
                                request.SectionCode,
                                request.UserID));
                        }
                        catch (Exception auditEx)
                        {
                            Console.WriteLine($"[AUDIT] Logging failed for specimen {request.SpecimenNo}: {auditEx.Message}");
                        }
                    }

                    // ── Section cut-off time ───────────────────────────────────────
                    var section = context.Section_Master
                        .FirstOrDefault(s => s.Code == request.SectionCode);
                    string? cutOffTimeStr = section?.CutOffTime.HasValue == true
                        ? $"{(int)section.CutOffTime.Value.TotalHours:D2}:{section.CutOffTime.Value.Minutes:D2}"
                        : null;

                    var todayDayName = DateTime.Now.DayOfWeek.ToString();

                    var testCodes = tests.Select(t => t.TestCode).ToList();
                    var allRunningDayRecords = context.Test_RunningDay
                      .ToList()
                      .Where(r => testCodes.Contains(r.TestCode))
                      .ToList();

                    var testCodesWithRunningDay = allRunningDayRecords
                        .Select(r => r.TestCode)
                        .ToHashSet();

                    var testCodesRunningToday = allRunningDayRecords
                        .Where(r => r.RunningDays != null &&
                                    r.RunningDays.Split(';').Contains(todayDayName))
                        .Select(r => r.TestCode)
                        .ToHashSet();

                    var sampleTypeName = context.Sample_Type
                        .FirstOrDefault(s => s.Code == header.SampleTypeCode)?.Name;

                    var batchSpecimen = temp != null ?
                        temp : context.Batch_Specimen
                        .FirstOrDefault(b => b.SpecimenNo == header.SpecimenNo);

                    using var runningDayMaster = new MasterService(_branch_raw);
                    var nextRunningDates = tests
                        .Where(t => testCodesWithRunningDay.Contains(t.TestCode))
                        .ToDictionary(
                            t => t.TestCode,
                            t => runningDayMaster.TestRunningDay
                                    .GetNearestRunningDate(t.TestCode, DateOnly.FromDateTime(now).AddDays(1))
                        );


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
                        PatientName = batchSpecimen?.PatientName,
                        PID = batchSpecimen?.PID,
                        CutOffTime = cutOffTimeStr,
                        SpecimenAlert = specimenAlert,
                        SpecimenAlertSetBy = specimenAlertSetBy,
                        SpecimenAlertSetAt = specimenAlertSetAt,
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
                            HasRunningDay = testCodesWithRunningDay.Contains(t.TestCode),
                            IsTodayRunningDay = testCodesRunningToday.Contains(t.TestCode),
                            NextRunningDate = nextRunningDates.TryGetValue(t.TestCode, out var nrd) ? nrd : null
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
        public SaveAssignmentsResult SaveAssignments(SaveAssignmentsRequest request)
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

                    // ── Track tests actually flipped to Running this save ──────────
                    var justRunTests = new List<(Specimen_Section_Test Test, string AssignedRMT)>();
                    var justScheduledTests = new List<(Specimen_Section_Test Test, string? PreviousTag)>();

                    // ── Instantiate running day service once for the whole batch ───
                    using var master = new MasterService(_branch_raw);

                    var srdFrom = today.AddDays(1);

                    foreach (var assignment in request.Assignments)
                    {
                        var test = tests.FirstOrDefault(t => t.Id == assignment.TestId);
                        if (test == null) continue;

                        // Skip tests that are already Running or Released
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
                                        .GetNearestRunningDate(test.TestCode, srdFrom); // ← was: today

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
                            // Today — no tag, no date
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

                    var justCompletedHeaders = new List<CompletedHeaderInfo>();
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

                        if (newStatus == "C" && header.Status != "C")
                        {
                            justCompletedHeaders.Add(new CompletedHeaderInfo
                            {
                                SpecimenNo = header.SpecimenNo,
                                SectionCode = header.SectionCode
                            });
                        }
                    }

                    context.SaveChanges();
                    tx.Commit();

                    // ── Audit Logging ──────────────────────────────────────────────
                    try
                    {
                        using var auditMaster = new MasterService(_branch_raw);

                        var auditSpecimenNos = headers.Select(h => h.SpecimenNo).ToHashSet();
                        var batchSpecimenMap = context.Batch_Specimen
                            .ToList()
                            .Where(b => auditSpecimenNos.Contains(b.SpecimenNo))
                            .GroupBy(b => b.SpecimenNo)
                            .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First());

                        // ── TEST_RUN ───────────────────────────────────────────────
                        var runGroups = justRunTests
                            .GroupBy(x => x.Test.HeaderId)
                            .ToList();

                        foreach (var group in runGroups)
                        {
                            var header = headers.FirstOrDefault(h => h.Id == group.Key);
                            if (header == null) continue;

                            batchSpecimenMap.TryGetValue(header.SpecimenNo, out var bs);
                            var testNames = string.Join(",", group.Select(x => $"{x.Test.TestCode}:{x.Test.TestName}"));
                            var rmtUserID = group.First().AssignedRMT;

                            auditMaster.Audit.Log(Audit_Log.TestRun(
                                header.SpecimenNo,
                                bs?.PatientName ?? string.Empty,
                                bs?.PID ?? string.Empty,
                                header.SectionCode,
                                testNames,
                                rmtUserID,
                                request.UserID,
                                now));
                        }

                        // ── SPECIMEN_STORED / TEST_SCHEDULED / TEST_RESCHEDULED ────
                        var scheduledByHeader = justScheduledTests
                            .GroupBy(x => x.Test.HeaderId)
                            .ToList();

                        foreach (var group in scheduledByHeader)
                        {
                            var header = headers.FirstOrDefault(h => h.Id == group.Key);
                            if (header == null) continue;

                            batchSpecimenMap.TryGetValue(header.SpecimenNo, out var bs);
                            var patientName = bs?.PatientName ?? string.Empty;
                            var pid = bs?.PID ?? string.Empty;

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
                        Console.WriteLine($"[AUDIT] Logging failed in SaveAssignments: {auditEx.Message}");
                    }

                    return new SaveAssignmentsResult
                    {
                        CompletedHeaders = justCompletedHeaders
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

        #region backup code / old one (cutoff srd issue)
        //public SaveAssignmentsResult SaveAssignments(SaveAssignmentsRequest request)
        //{
        //    try
        //    {
        //        using var context = _factory.CreateContext(_branch);
        //        using var tx = context.Database.BeginTransaction();
        //        try
        //        {
        //            var now = DateTime.Now;
        //            var today = DateOnly.FromDateTime(now);
        //            var testIds = new HashSet<int>(request.Assignments.Select(a => a.TestId));

        //            // Load all tests in memory first — avoids EF Core CTE syntax error with Contains()
        //            var tests = context.Specimen_Section_Test
        //                .ToList()
        //                .Where(t => testIds.Contains(t.Id))
        //                .ToList();

        //            if (!tests.Any())
        //                throw new Exception("No matching tests found.");

        //            // ── Track tests actually flipped to Running this save ──────────
        //            var justRunTests = new List<(Specimen_Section_Test Test, string AssignedRMT)>();
        //            var justScheduledTests = new List<(Specimen_Section_Test Test, string? PreviousTag)>();

        //            // ── Instantiate running day service once for the whole batch ───
        //            using var master = new MasterService(_branch_raw);

        //            foreach (var assignment in request.Assignments)
        //            {
        //                var test = tests.FirstOrDefault(t => t.Id == assignment.TestId);
        //                if (test == null) continue;

        //                // Skip tests that are already Running or Released
        //                if (test.Status == "R" || test.Status == "X") continue;
        //                var previousTag = test.ScheduleTag;

        //                test.UpdatedBy = request.UserID;
        //                test.Updated = now;

        //                if (!string.IsNullOrEmpty(assignment.ScheduleTag))
        //                {
        //                    switch (assignment.ScheduleTag)
        //                    {
        //                        case "END":
        //                            test.ScheduleTag = "END";
        //                            test.RunningDate = DateOnly.FromDateTime(now.AddDays(1));
        //                            test.Status = "S";
        //                            test.AssignedRMT = assignment.AssignedRMT;
        //                            test.Assigned = null;
        //                            justScheduledTests.Add((test, previousTag));
        //                            break;

        //                        case "CRD":
        //                            if (assignment.RunningDate == null)
        //                                throw new Exception(
        //                                    $"Test '{test.TestCode}': A running date is required for CRD.");

        //                            if (assignment.RunningDate <= DateOnly.FromDateTime(now))
        //                                throw new Exception(
        //                                    $"Test '{test.TestCode}': CRD running date must be a future date.");

        //                            test.ScheduleTag = "CRD";
        //                            test.RunningDate = assignment.RunningDate;
        //                            test.Status = "S";
        //                            test.AssignedRMT = assignment.AssignedRMT;
        //                            test.Assigned = null;
        //                            justScheduledTests.Add((test, previousTag));
        //                            break;

        //                        case "SRD":
        //                            var nearestDate = master.TestRunningDay
        //                                .GetNearestRunningDate(test.TestCode, today);

        //                            if (nearestDate == null)
        //                                throw new Exception(
        //                                    $"Test '{test.TestCode}' has no running day configured. " +
        //                                    $"Please set it up in Admin Settings → Running Days before using SRD.");

        //                            test.ScheduleTag = "SRD";
        //                            test.RunningDate = nearestDate;
        //                            test.Status = "S";
        //                            test.AssignedRMT = assignment.AssignedRMT;
        //                            test.Assigned = null;
        //                            justScheduledTests.Add((test, previousTag));
        //                            break;
        //                    }
        //                }
        //                else
        //                {
        //                    // Today — no tag, no date
        //                    test.ScheduleTag = null;
        //                    test.RunningDate = null;

        //                    if (!string.IsNullOrEmpty(assignment.AssignedRMT))
        //                    {
        //                        test.Status = "R";
        //                        test.AssignedRMT = assignment.AssignedRMT;
        //                        test.Assigned = now;
        //                        test.RunAt = now;

        //                        // ← Track this test as just run
        //                        justRunTests.Add((test, assignment.AssignedRMT));
        //                    }
        //                    else
        //                    {
        //                        test.Status = "P";
        //                        test.AssignedRMT = null;
        //                        test.Assigned = null;
        //                    }
        //                }

        //                context.Specimen_Section_Test.Update(test);
        //            }

        //            // Update specimen remarks
        //            if (request.SpecimenRemarks?.Any() == true)
        //            {
        //                var headerIds = request.SpecimenRemarks.Select(r => r.HeaderId).ToHashSet();
        //                var _headers = context.Specimen_Section_Header
        //                    .ToList()
        //                    .Where(h => headerIds.Contains(h.Id))
        //                    .ToList();

        //                foreach (var remarkItem in request.SpecimenRemarks)
        //                {
        //                    var header = _headers.FirstOrDefault(h => h.Id == remarkItem.HeaderId);
        //                    if (header == null) continue;
        //                    header.Remarks = remarkItem.Remarks;
        //                    header.UpdatedBy = request.UserID;
        //                    header.Updated = now;
        //                    context.Specimen_Section_Header.Update(header);
        //                }
        //            }

        //            var affectedHeaderIds = tests
        //                .Where(t => request.Assignments.Any(a => a.TestId == t.Id))
        //                .Select(t => t.HeaderId)
        //                .Distinct()
        //                .ToList();

        //            var allTestsForHeaders = context.Specimen_Section_Test
        //                .ToList()
        //                .Where(t => affectedHeaderIds.Contains(t.HeaderId))
        //                .ToList();

        //            var headers = context.Specimen_Section_Header
        //                .ToList()
        //                .Where(h => affectedHeaderIds.Contains(h.Id))
        //                .ToList();

        //            var justCompletedHeaders = new List<CompletedHeaderInfo>();
        //            foreach (var header in headers)
        //            {
        //                var headerTests = allTestsForHeaders.Where(t => t.HeaderId == header.Id).ToList();

        //                string newStatus;
        //                if (headerTests.All(t => t.Status == "X"))
        //                    newStatus = "C";
        //                else if (headerTests.Any(t => t.Status == "R" || t.Status == "S" || t.Status == "X"))
        //                    newStatus = "S";
        //                else
        //                    newStatus = "P";

        //                if (header.Status != newStatus)
        //                {
        //                    header.Status = newStatus;
        //                    header.UpdatedBy = request.UserID;
        //                    header.Updated = now;
        //                    context.Specimen_Section_Header.Update(header);
        //                }

        //                if (newStatus == "C" && header.Status != "C") // was not already C
        //                {
        //                    justCompletedHeaders.Add(new CompletedHeaderInfo
        //                    {
        //                        SpecimenNo = header.SpecimenNo,
        //                        SectionCode = header.SectionCode
        //                    });
        //                }
        //            }

        //            context.SaveChanges();
        //            tx.Commit();

        //            // ── Audit Logging ──────────────────────────────────────────────────────
        //            try
        //            {
        //                using var auditMaster = new MasterService(_branch_raw);

        //                // ── Pre-load Batch_Specimen for all affected headers in one query ──
        //                // Avoids a per-header DB hit inside the loops below.
        //                var auditSpecimenNos = headers.Select(h => h.SpecimenNo).ToHashSet();
        //                var batchSpecimenMap = context.Batch_Specimen
        //                    .ToList()
        //                    .Where(b => auditSpecimenNos.Contains(b.SpecimenNo))
        //                    .GroupBy(b => b.SpecimenNo)
        //                    .ToDictionary(g => g.Key, g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First());

        //                // ── TEST_RUN — one entry per specimen for tests flipped to R ──────
        //                var runGroups = justRunTests
        //                    .GroupBy(x => x.Test.HeaderId)
        //                    .ToList();

        //                foreach (var group in runGroups)
        //                {
        //                    var header = headers.FirstOrDefault(h => h.Id == group.Key);
        //                    if (header == null) continue;

        //                    batchSpecimenMap.TryGetValue(header.SpecimenNo, out var bs);
        //                    var testNames = string.Join(",", group.Select(x => $"{x.Test.TestCode}:{x.Test.TestName}"));
        //                    var rmtUserID = group.First().AssignedRMT;

        //                    auditMaster.Audit.Log(Audit_Log.TestRun(
        //                        header.SpecimenNo,
        //                        bs?.PatientName ?? string.Empty,
        //                        bs?.PID ?? string.Empty,
        //                        header.SectionCode,
        //                        testNames,
        //                        rmtUserID,
        //                        request.UserID,
        //                        now));
        //                }

        //                // ── SPECIMEN_STORED / TEST_SCHEDULED / TEST_RESCHEDULED ───────────
        //                var scheduledByHeader = justScheduledTests
        //                    .GroupBy(x => x.Test.HeaderId)
        //                    .ToList();

        //                foreach (var group in scheduledByHeader)
        //                {
        //                    var header = headers.FirstOrDefault(h => h.Id == group.Key);
        //                    if (header == null) continue;

        //                    batchSpecimenMap.TryGetValue(header.SpecimenNo, out var bs);
        //                    var patientName = bs?.PatientName ?? string.Empty;
        //                    var pid = bs?.PID ?? string.Empty;

        //                    // Fire SPECIMEN_STORED once per specimen
        //                    auditMaster.Audit.Log(Audit_Log.SpecimenStored(
        //                        header.SpecimenNo,
        //                        patientName,
        //                        pid,
        //                        header.SectionCode,
        //                        request.UserID));

        //                    // Fire TEST_SCHEDULED or TEST_RESCHEDULED per test
        //                    foreach (var item in group)
        //                    {
        //                        var runningDate = item.Test.RunningDate.HasValue
        //                            ? item.Test.RunningDate.Value.ToDateTime(TimeOnly.MinValue)
        //                            : DateTime.Today;
        //                        var testEntry = $"{item.Test.TestCode}:{item.Test.TestName}";

        //                        if (item.PreviousTag == null)
        //                        {
        //                            auditMaster.Audit.Log(Audit_Log.TestScheduled(
        //                                header.SpecimenNo,
        //                                patientName,
        //                                pid,
        //                                header.SectionCode,
        //                                testEntry,
        //                                item.Test.ScheduleTag!,
        //                                runningDate,
        //                                request.UserID));
        //                        }
        //                        else if (item.PreviousTag != item.Test.ScheduleTag)
        //                        {
        //                            auditMaster.Audit.Log(Audit_Log.TestRescheduled(
        //                                header.SpecimenNo,
        //                                patientName,
        //                                pid,
        //                                header.SectionCode,
        //                                testEntry,
        //                                item.PreviousTag,
        //                                item.Test.ScheduleTag!,
        //                                runningDate,
        //                                request.UserID));
        //                        }
        //                    }
        //                }
        //            }
        //            catch (Exception auditEx)
        //            {
        //                Console.WriteLine($"[AUDIT] Logging failed in SaveAssignments: {auditEx.Message}");
        //            }

        //            return new SaveAssignmentsResult
        //            {
        //                CompletedHeaders = justCompletedHeaders
        //            };
        //        }
        //        catch
        //        {
        //            tx.Rollback();
        //            throw;
        //        }
        //    }
        //    catch { throw; }
        //}

        #endregion

        public List<PendingSpecimenItem> GetPendingSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var today = DateOnly.FromDateTime(DateTime.Today);

                // ── Standard: fully pending headers (Status = P) ───────────────────
                var pendingHeaders = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode
                             && h.Status == "P"
                             && h.IsHclabRouted)
                    .ToList();

                // ── Standard: mixed headers (Status = S) with at least one due test ─
                // These are headers that still have future SRD tests (so not flipped
                // to P by middleware) but also have tests whose RunningDate is today or past.
                var scheduledHeaders = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode
                             && h.Status == "S"
                             && h.IsHclabRouted)
                    .ToList();

                var scheduledHeaderIds = scheduledHeaders.Select(h => h.Id).ToList();

                var dueTestHeaderIds = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => scheduledHeaderIds.Contains(t.HeaderId)
                             && t.Status == "S"
                             && t.RunningDate.HasValue
                             && t.RunningDate.Value <= today)
                    .Select(t => t.HeaderId)
                    .Distinct()
                    .ToHashSet();

                var mixedHeaders = scheduledHeaders
                    .Where(h => dueTestHeaderIds.Contains(h.Id))
                    .ToList();

                // ── Merge, deduplicate, sort ───────────────────────────────────────
                var headers = pendingHeaders
                    .Concat(mixedHeaders)
                    .GroupBy(h => h.Id)
                    .Select(g => g.First())
                    .OrderByDescending(h => h.Routed)
                    .ToList();

                var specimenNos = headers.Select(h => h.SpecimenNo).ToList();
                var batchSpecimens = context.Batch_Specimen
                    .ToList()
                    .Where(b => specimenNos.Contains(b.SpecimenNo))
                    .GroupBy(b => b.SpecimenNo)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First()
                    );

                var standard = headers.Select(h =>
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

                // ── On-Site: fully pending ─────────────────────────────────────────
                var onSiteHeaders = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode && h.Status == "P")
                    .ToList();

                var onSite = onSiteHeaders.Select(h => new PendingSpecimenItem
                {
                    Id = h.Id,
                    SpecimenNo = h.SpecimenNo,
                    SectionCode = h.SectionCode,
                    TestGroupCode = h.TestGroupCode,
                    SampleTypeCode = h.SampleTypeCode,
                    SampleTypeName = unit.SampleTypes.GetByCode(h.SampleTypeCode)?.Name ?? h.SampleTypeCode,
                    Status = h.Status,
                    ReceivedBy = h.ReceivedBy,
                    Received = h.Received,
                    Remarks = h.Remarks,
                    PatientName = h.PatientName,
                    PatientID = h.PID,
                    IsOnSite = true
                }).ToList();

                return standard
                    .Concat(onSite)
                    .OrderByDescending(s => s.Received)
                    .ToList();
            }
            catch { throw; }
        }

        public List<ScheduledSpecimenItem> GetScheduledSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var today = DateOnly.FromDateTime(DateTime.Today);

                // ── Standard specimens ─────────────────────────────────────────────
                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();

                var headerIds = headers.Select(h => h.Id).ToList();

                var allSavedTests = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => headerIds.Contains(t.HeaderId) && t.Status == "S")
                    .ToList();

                // ── Only show tests that are still future-dated ────────────────────
                // Due tests (RunningDate <= today) belong in Pending, not Scheduled.
                var futureTests = allSavedTests
                    .Where(t => t.RunningDate.HasValue && t.RunningDate.Value > today)
                    .ToList();

                var relevantHeaderIds = futureTests.Select(t => t.HeaderId).Distinct().ToHashSet();
                var relevantHeaders = headers.Where(h => relevantHeaderIds.Contains(h.Id)).ToList();

                var specimenNos = relevantHeaders.Select(h => h.SpecimenNo).ToList();
                var batchSpecimens = context.Batch_Specimen
                    .ToList()
                    .Where(b => specimenNos.Contains(b.SpecimenNo))
                    .GroupBy(b => b.SpecimenNo)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First()
                    );

                var standard = relevantHeaders.Select(h =>
                {
                    batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
                    var tests = futureTests
                        .Where(t => t.HeaderId == h.Id)
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
                }).ToList();

                // ── On-Site specimens ──────────────────────────────────────────────
                var onSiteHeaders = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode && h.Status == "S")
                    .ToList();

                var onSiteHeaderIds = onSiteHeaders.Select(h => h.Id).ToList();

                var allOnSiteSavedTests = context.OnSite_Section_Test
                    .ToList()
                    .Where(t => onSiteHeaderIds.Contains(t.HeaderId) && t.Status == "S")
                    .ToList();

                var futureOnSiteTests = allOnSiteSavedTests
                    .Where(t => t.RunningDate.HasValue && t.RunningDate.Value > today)
                    .ToList();

                var relevantOnSiteHeaderIds = futureOnSiteTests.Select(t => t.HeaderId).Distinct().ToHashSet();

                var onSite = onSiteHeaders
                    .Where(h => relevantOnSiteHeaderIds.Contains(h.Id))
                    .Select(h =>
                    {
                        var tests = futureOnSiteTests
                            .Where(t => t.HeaderId == h.Id)
                            .OrderBy(t => t.RunningDate)
                            .ThenBy(t => t.TestCode)
                            .ToList();

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
                            IsOnSite = true,
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
                    }).ToList();

                return standard
                    .Concat(onSite)
                    .OrderBy(s => s.Tests.Min(t => t.RunningDate ?? DateOnly.MaxValue))
                    .ToList();
            }
            catch { throw; }
        }

        #region back up getpending specimens & scheduled specimens

        // ── Get pending specimens ──────────────────────────────────────────

        //public List<PendingSpecimenItem> GetPendingSpecimens(string sectionCode)
        //{
        //    try
        //    {
        //        using var context = _factory.CreateContext(_branch);
        //        using var unit = new UnitOfWork(context);

        //        // ── Standard specimens ─────────────────────────────────────────
        //        var headers = context.Specimen_Section_Header
        //            .Where(h => h.SectionCode == sectionCode
        //                     && h.Status == "P"
        //                     && h.IsHclabRouted)
        //            .OrderByDescending(h => h.Routed)
        //            .ToList();

        //        var specimenNos = headers.Select(h => h.SpecimenNo).ToList();
        //        var batchSpecimens = context.Batch_Specimen
        //                          .ToList()
        //                          .Where(b => specimenNos.Contains(b.SpecimenNo))
        //                          .GroupBy(b => b.SpecimenNo)
        //                          .ToDictionary(
        //                              g => g.Key,
        //                              g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First()
        //                          );

        //        var standard = headers.Select(h =>
        //        {
        //            batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
        //            return new PendingSpecimenItem
        //            {
        //                Id = h.Id,
        //                SpecimenNo = h.SpecimenNo,
        //                SectionCode = h.SectionCode,
        //                TestGroupCode = h.TestGroupCode,
        //                SampleTypeCode = h.SampleTypeCode,
        //                SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
        //                Status = h.Status,
        //                RoutedBy = h.RoutedBy,
        //                Routed = h.Routed,
        //                ReceivedBy = h.ReceivedBy,
        //                Received = h.Received,
        //                Remarks = h.Remarks,
        //                PatientName = bs?.PatientName,
        //                PatientID = bs?.PID,
        //            };
        //        }).ToList();

        //        // ── On-Site specimens ──────────────────────────────────────────
        //        var onSiteHeaders = context.OnSite_Section_Header
        //            .Where(h => h.SectionCode == sectionCode && h.Status == "P")
        //            .ToList();

        //        var onSite = onSiteHeaders.Select(h => new PendingSpecimenItem
        //        {
        //            Id = h.Id,
        //            SpecimenNo = h.SpecimenNo,
        //            SectionCode = h.SectionCode,
        //            TestGroupCode = h.TestGroupCode,
        //            SampleTypeCode = h.SampleTypeCode,
        //            SampleTypeName = unit.SampleTypes.GetByCode(h.SampleTypeCode)?.Name ?? h.SampleTypeCode,
        //            Status = h.Status,
        //            ReceivedBy = h.ReceivedBy,
        //            Received = h.Received,
        //            Remarks = h.Remarks,
        //            PatientName = h.PatientName,
        //            PatientID = h.PID,
        //            IsOnSite = true
        //        }).ToList();

        //        // ── Merge and sort by received descending ──────────────────────
        //        return standard
        //            .Concat(onSite)
        //            .OrderByDescending(s => s.Received)
        //            .ToList();
        //    }
        //    catch { throw; }
        //}

        //// ── Get scheduled specimens ────────────────────────────────────────────
        //public List<ScheduledSpecimenItem> GetScheduledSpecimens(string sectionCode)
        //{
        //    try
        //    {
        //        using var context = _factory.CreateContext(_branch);
        //        using var unit = new UnitOfWork(context);

        //        // ── Standard specimens ─────────────────────────────────────────
        //        var headers = context.Specimen_Section_Header
        //            .Where(h => h.SectionCode == sectionCode)
        //            .ToList();

        //        var headerIds = headers.Select(h => h.Id).ToList();

        //        var allTests = context.Specimen_Section_Test
        //            .ToList()
        //            .Where(t => headerIds.Contains(t.HeaderId) && t.Status == "S")
        //            .ToList();

        //        var relevantHeaderIds = allTests.Select(t => t.HeaderId).Distinct().ToHashSet();
        //        var relevantHeaders = headers.Where(h => relevantHeaderIds.Contains(h.Id)).ToList();

        //        var specimenNos = relevantHeaders.Select(h => h.SpecimenNo).ToList();

        //        var batchSpecimens = context.Batch_Specimen
        //               .ToList()
        //               .Where(b => specimenNos.Contains(b.SpecimenNo))
        //               .GroupBy(b => b.SpecimenNo)
        //               .ToDictionary(
        //                   g => g.Key,
        //                   g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First()
        //               );

        //        var standard = relevantHeaders.Select(h =>
        //        {
        //            batchSpecimens.TryGetValue(h.SpecimenNo, out var bs);
        //            var tests = allTests.Where(t => t.HeaderId == h.Id)
        //                .OrderBy(t => t.RunningDate)
        //                .ThenBy(t => t.TestCode)
        //                .ToList();

        //            return new ScheduledSpecimenItem
        //            {
        //                HeaderId = h.Id,
        //                SpecimenNo = h.SpecimenNo,
        //                SectionCode = h.SectionCode,
        //                SampleTypeCode = h.SampleTypeCode,
        //                SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
        //                PatientName = bs?.PatientName,
        //                PatientID = bs?.PID,
        //                Remarks = h.Remarks,
        //                ReceivedBy = h.ReceivedBy,
        //                Received = h.Received,
        //                Tests = tests.Select(t => new ScheduledTestItem
        //                {
        //                    Id = t.Id,
        //                    TestCode = t.TestCode,
        //                    TestName = t.TestName,
        //                    Status = t.Status,
        //                    ScheduleTag = t.ScheduleTag,
        //                    RunningDate = t.RunningDate,
        //                    AssignedRMT = t.AssignedRMT,
        //                }).ToList()
        //            };
        //        }).ToList();

        //        // ── On-Site specimens ──────────────────────────────────────────
        //        var onSiteHeaders = context.OnSite_Section_Header
        //            .Where(h => h.SectionCode == sectionCode)
        //            .ToList();

        //        var onSiteHeaderIds = onSiteHeaders.Select(h => h.Id).ToList();

        //        var onSiteTests = context.OnSite_Section_Test
        //            .ToList()
        //            .Where(t => onSiteHeaderIds.Contains(t.HeaderId) && t.Status == "S")
        //            .ToList();

        //        var relevantOnSiteHeaderIds = onSiteTests.Select(t => t.HeaderId).Distinct().ToHashSet();

        //        var onSite = onSiteHeaders
        //            .Where(h => relevantOnSiteHeaderIds.Contains(h.Id))
        //            .Select(h =>
        //            {
        //                var tests = onSiteTests.Where(t => t.HeaderId == h.Id)
        //                    .OrderBy(t => t.RunningDate)
        //                    .ThenBy(t => t.TestCode)
        //                    .ToList();

        //                return new ScheduledSpecimenItem
        //                {
        //                    HeaderId = h.Id,
        //                    SpecimenNo = h.SpecimenNo,
        //                    SectionCode = h.SectionCode,
        //                    SampleTypeCode = h.SampleTypeCode,
        //                    SampleTypeName = unit.SampleTypes.GetByCode(h.SampleTypeCode)?.Name ?? h.SampleTypeCode,
        //                    PatientName = h.PatientName,
        //                    PatientID = h.PID,
        //                    Remarks = h.Remarks,
        //                    ReceivedBy = h.ReceivedBy,
        //                    Received = h.Received,
        //                    IsOnSite = true,
        //                    Tests = tests.Select(t => new ScheduledTestItem
        //                    {
        //                        Id = t.Id,
        //                        TestCode = t.TestCode,
        //                        TestName = t.TestName,
        //                        Status = t.Status,
        //                        ScheduleTag = t.ScheduleTag,
        //                        RunningDate = t.RunningDate,
        //                        AssignedRMT = t.AssignedRMT,
        //                    }).ToList()
        //                };
        //            }).ToList();

        //        // ── Merge and sort by nearest running date ─────────────────────
        //        return standard
        //            .Concat(onSite)
        //            .OrderBy(s => s.Tests.Min(t => t.RunningDate ?? DateOnly.MaxValue))
        //            .ToList();
        //    }
        //    catch { throw; }
        //}

        #endregion

        public List<RunningSpecimenItem> GetRunningSpecimens(string sectionCode, string? userID)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                // ── Standard specimens ─────────────────────────────────────────
                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();

                var headerIds = headers.Select(h => h.Id).ToList();

                var runningTests = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => headerIds.Contains(t.HeaderId)
                             && t.Status == "R"
                             && (userID == null || t.AssignedRMT == userID))
                    .ToList();

                var relevantHeaderIds = runningTests.Select(t => t.HeaderId).Distinct().ToHashSet();
                var relevantHeaders = headers.Where(h => relevantHeaderIds.Contains(h.Id)).ToList();

                var specimenNos = relevantHeaders.Select(h => h.SpecimenNo).ToList();

                var batchSpecimens = context.Batch_Specimen
                       .ToList()
                       .Where(b => specimenNos.Contains(b.SpecimenNo))
                       .GroupBy(b => b.SpecimenNo)
                       .ToDictionary(
                           g => g.Key,
                           g => g.OrderBy(b => b.Status == "X" ? 1 : 0).First()
                       );

                var standard = relevantHeaders.Select(h =>
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
                }).ToList();

                // ── On-Site specimens ──────────────────────────────────────────
                var onSiteHeaders = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();

                var onSiteHeaderIds = onSiteHeaders.Select(h => h.Id).ToList();

                var onSiteRunningTests = context.OnSite_Section_Test
                    .ToList()
                    .Where(t => onSiteHeaderIds.Contains(t.HeaderId)
                             && t.Status == "R"
                             && (userID == null || t.AssignedRMT == userID))
                    .ToList();

                var relevantOnSiteHeaderIds = onSiteRunningTests.Select(t => t.HeaderId).Distinct().ToHashSet();

                var onSite = onSiteHeaders
                    .Where(h => relevantOnSiteHeaderIds.Contains(h.Id))
                    .Select(h =>
                    {
                        var tests = onSiteRunningTests
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
                            SampleTypeName = unit.SampleTypes.GetByCode(h.SampleTypeCode)?.Name ?? h.SampleTypeCode,
                            PatientName = h.PatientName,
                            PatientID = h.PID,
                            Remarks = h.Remarks,
                            Received = h.Received,
                            IsOnSite = true,
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
                    }).ToList();

                // ── Merge and sort by earliest RunAt ───────────────────────────
                return standard
                    .Concat(onSite)
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
                var today = DateOnly.FromDateTime(DateTime.Today);

                // ── Standard ───────────────────────────────────────────────────
                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode && h.IsHclabRouted)
                    .ToList();
                var headerIds = headers.Select(h => h.Id).ToList();
                var allTests = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => headerIds.Contains(t.HeaderId))
                    .ToList();

                // ── On-Site ────────────────────────────────────────────────────
                var onSiteHeaders = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .ToList();
                var onSiteHeaderIds = onSiteHeaders.Select(h => h.Id).ToList();
                var onSiteTests = context.OnSite_Section_Test
                    .ToList()
                    .Where(t => onSiteHeaderIds.Contains(t.HeaderId))
                    .ToList();

                return new RunnerDashboardSummary
                {
                    Pending = headers.Count(h => h.Status == "P" || h.Status == "S" || h.Status == "R")
                            + onSiteHeaders.Count(h => h.Status == "P" || h.Status == "S" || h.Status == "R"),

                    Scheduled = allTests.Count(t => t.Status == "S")
                              + onSiteTests.Count(t => t.Status == "S"),

                    Running = allTests.Count(t => t.Status == "R" && t.AssignedRMT == userID)
                            + onSiteTests.Count(t => t.Status == "R" && t.AssignedRMT == userID),

                    CompletedToday = headers.Count(h =>
                                        h.Status == "C" &&
                                        h.Updated.HasValue &&
                                        DateOnly.FromDateTime(h.Updated.Value) == today)
                                   + onSiteHeaders.Count(h =>
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
                    var completed = GetCompletedToday(section.Code);
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
            using var unit = new UnitOfWork(context);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var sampleTypes = unit.SampleTypes.GetActive();

            // ── Standard ───────────────────────────────────────────────────
            var headers = context.Specimen_Section_Header
                .Where(h => h.SectionCode == sectionCode
                         && h.Status == "C"
                         && h.Updated.HasValue
                         && DateOnly.FromDateTime(h.Updated.Value) == today)
                .OrderByDescending(h => h.Updated)
                .ToList();

            var specimenNos = headers.Select(h => h.SpecimenNo).ToList();
            var batchSpecimens = context.Batch_Specimen
                .ToList()
                .Where(b => specimenNos.Contains(b.SpecimenNo))
                .GroupBy(b => b.SpecimenNo)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.Id).First());

            var standard = headers.Select(h =>
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
                    IsOnSite = false,
                };
            }).ToList();

            // ── On-Site ────────────────────────────────────────────────────
            var onSiteHeaders = context.OnSite_Section_Header
                .Where(h => h.SectionCode == sectionCode
                         && h.Status == "C"
                         && h.Updated.HasValue
                         && DateOnly.FromDateTime(h.Updated.Value) == today)
                .OrderByDescending(h => h.Updated)
                .ToList();

            var onSite = onSiteHeaders.Select(h => new PendingSpecimenItem
            {
                Id = h.Id,
                SpecimenNo = h.SpecimenNo,
                SectionCode = h.SectionCode,
                TestGroupCode = h.TestGroupCode,
                SampleTypeCode = h.SampleTypeCode,
                SampleTypeName = sampleTypes.FirstOrDefault(s => s.Code == h.SampleTypeCode)?.Name ?? h.SampleTypeCode,
                Status = h.Status,
                ReceivedBy = h.ReceivedBy,
                Received = h.Received,
                Remarks = h.Remarks,
                PatientName = h.PatientName,
                PatientID = h.PID,
                IsOnSite = true,
            }).ToList();

            return standard
                .Concat(onSite)
                .OrderByDescending(s => s.Received)
                .ToList();
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

        public List<CompletedSpecimenItem> GetCompletedSpecimens(string sectionCode, DateTime from, DateTime to)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var toInclusive = to.Date.AddDays(1);
                var sampleTypes = unit.SampleTypes.GetActive();

                // ── Standard ───────────────────────────────────────────────────
                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode
                             && h.Status == "C"
                             && h.Updated >= from.Date
                             && h.Updated < toInclusive)
                    .OrderByDescending(h => h.Updated)
                    .ToList();

                var headerIds = headers.Select(h => h.Id).ToList();
                var specimenNos = headers.Select(h => h.SpecimenNo).ToList();

                var tests = context.Specimen_Section_Test
                    .ToList()
                    .Where(t => headerIds.Contains(t.HeaderId))
                    .ToList();

                var allSpecimens = context.Batch_Specimen
                    .Where(s => s.TrxDate >= from.Date.AddMonths(-3))
                    .ToList();
                var specimens = allSpecimens
                    .Where(s => specimenNos.Contains(s.SpecimenNo))
                    .GroupBy(b => b.SpecimenNo)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(b => b.Id).First());

                var standard = headers.Select(h =>
                {
                    specimens.TryGetValue(h.SpecimenNo, out var bs);
                    return new CompletedSpecimenItem
                    {
                        HeaderId = h.Id,
                        SpecimenNo = h.SpecimenNo,
                        PID = bs?.PID ?? string.Empty,
                        PatientName = bs?.PatientName ?? string.Empty,
                        SampleTypeCode = h.SampleTypeCode,
                        SampleTypeName = bs?.SampleTypeName ?? h.SampleTypeCode,
                        Received = h.Received,
                        ReceivedBy = h.ReceivedBy,
                        Completed = h.Updated,
                        CompletedBy = h.UpdatedBy,
                        IsOnSite = false,
                        Tests = tests
                            .Where(t => t.HeaderId == h.Id)
                            .Select(t => new CompletedTestItem
                            {
                                Id = t.Id,
                                TestCode = t.TestCode,
                                TestName = t.TestName,
                                AssignedRMT = t.AssignedRMT,
                                RunAt = t.RunAt,
                                Assigned = t.Assigned,
                            }).ToList()
                    };
                }).ToList();

                // ── On-Site ────────────────────────────────────────────────────
                var onSiteHeaders = context.OnSite_Section_Header
                    .Where(h => h.SectionCode == sectionCode
                             && h.Status == "C"
                             && h.Updated >= from.Date
                             && h.Updated < toInclusive)
                    .OrderByDescending(h => h.Updated)
                    .ToList();

                var onSiteHeaderIds = onSiteHeaders.Select(h => h.Id).ToList();

                var onSiteTests = context.OnSite_Section_Test
                    .ToList()
                    .Where(t => onSiteHeaderIds.Contains(t.HeaderId))
                    .ToList();

                var onSite = onSiteHeaders.Select(h => new CompletedSpecimenItem
                {
                    HeaderId = h.Id,
                    SpecimenNo = h.SpecimenNo,
                    PID = h.PID ?? string.Empty,
                    PatientName = h.PatientName ?? string.Empty,
                    SampleTypeCode = h.SampleTypeCode,
                    SampleTypeName = sampleTypes.FirstOrDefault(s => s.Code == h.SampleTypeCode)?.Name ?? h.SampleTypeCode,
                    Received = h.Received,
                    ReceivedBy = h.ReceivedBy,
                    Completed = h.Updated,
                    CompletedBy = h.UpdatedBy,
                    IsOnSite = true,
                    Tests = onSiteTests
                        .Where(t => t.HeaderId == h.Id)
                        .Select(t => new CompletedTestItem
                        {
                            Id = t.Id,
                            TestCode = t.TestCode,
                            TestName = t.TestName,
                            AssignedRMT = t.AssignedRMT,
                            RunAt = t.RunAt,
                            Assigned = t.Assigned,
                        }).ToList()
                }).ToList();

                // ── Merge ──────────────────────────────────────────────────────
                return standard
                    .Concat(onSite)
                    .OrderByDescending(s => s.Completed)
                    .ToList();
            }
            catch { throw; }
        }

        public List<SectionCompletedGroup> GetAllSectionsCompleted(DateTime from, DateTime to)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var sections = context.Section_Master
                    .Where(s => s.Category == "3" && s.Active)
                    .OrderBy(s => s.Name)
                    .ToList();

                var result = new List<SectionCompletedGroup>();

                foreach (var section in sections)
                {
                    var specimens = GetCompletedSpecimens(section.Code, from, to);
                    if (!specimens.Any()) continue;

                    result.Add(new SectionCompletedGroup
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

        // ── Cancel specimen (runner/section side) ──────────────────────────
        // TL / Admin only (enforced at controller level).
        // Sets header Status = "X" and cascades to all non-released child tests.
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
                    var header = context.Specimen_Section_Header
                        .FirstOrDefault(h => h.Id == request.HeaderId
                                          && h.SectionCode == request.SectionCode)
                        ?? throw new Exception(
                            $"Specimen '{request.SpecimenNo}' not found in section '{request.SectionCode}'.");

                    if (header.Status == "X")
                        throw new Exception($"Specimen '{request.SpecimenNo}' is already cancelled.");

                    if (header.Status == "C")
                        throw new Exception($"Specimen '{request.SpecimenNo}' is already completed and cannot be cancelled.");

                    // 2. Cancel the header
                    header.Status = "X";
                    header.UpdatedBy = request.UserID;
                    header.Updated = now;
                    context.Specimen_Section_Header.Update(header);

                    // 3. Cancel all non-released child tests
                    var tests = context.Specimen_Section_Test
                        .Where(t => t.HeaderId == header.Id && t.Status != "X")
                        .ToList();

                    foreach (var test in tests)
                    {
                        test.Status = "X";
                        test.UpdatedBy = request.UserID;
                        test.Updated = now;
                        context.Specimen_Section_Test.Update(test);
                    }

                    context.SaveChanges();
                    tx.Commit();

                    // 4. Audit — fire-and-forget
                    try
                    {
                        var bs = context.Batch_Specimen
                            .FirstOrDefault(b => b.SpecimenNo == request.SpecimenNo);

                        using var auditMaster = new MasterService(_branch_raw);
                        auditMaster.Audit.Log(Audit_Log.SpecimenCancelledSection(
                            request.SpecimenNo,
                            request.SectionCode,
                            bs?.PatientName ?? string.Empty,
                            bs?.PID ?? string.Empty,
                            request.Reason,
                            request.UserID));
                    }
                    catch (Exception auditEx)
                    {
                        Console.WriteLine($"[AUDIT] CancelSpecimen logging failed for {request.SpecimenNo}: {auditEx.Message}");
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

        // ── Abort a single running test back to pending ────────────────────
        // Available to all users.
        // Resets test Status R → P, clears assignment fields.
        // Re-derives header status from all sibling tests.
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
                    var test = context.Specimen_Section_Test
                        .FirstOrDefault(t => t.Id == request.TestId)
                        ?? throw new Exception($"Test ID {request.TestId} not found.");

                    if (test.Status != "R")
                        throw new Exception($"Only running tests can be aborted. Current status: {test.Status}.");

                    // 2. Reset the test
                    test.Status = "P";
                    test.AssignedRMT = null;
                    test.Assigned = null;
                    test.RunAt = null;
                    test.UpdatedBy = request.UserID;
                    test.Updated = now;
                    context.Specimen_Section_Test.Update(test);

                    // 3. Re-derive header status from all sibling tests
                    var header = context.Specimen_Section_Header
                        .FirstOrDefault(h => h.Id == request.HeaderId)
                        ?? throw new Exception($"Header ID {request.HeaderId} not found.");

                    var allSiblingTests = context.Specimen_Section_Test
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
                        header.UpdatedBy = request.UserID;
                        header.Updated = now;
                        context.Specimen_Section_Header.Update(header);
                    }

                    context.SaveChanges();
                    tx.Commit();

                    // 4. Audit — fire-and-forget
                    try
                    {
                        // Resolve patient info from Batch_Specimen
                        var bs = context.Batch_Specimen
                            .FirstOrDefault(b => b.SpecimenNo == request.SpecimenNo);

                        using var auditMaster = new MasterService(_branch_raw);
                        auditMaster.Audit.Log(Audit_Log.TestAborted(
                            request.SpecimenNo,
                            bs?.PatientName ?? string.Empty,
                            bs?.PID ?? string.Empty,
                            request.SectionCode,
                            test.TestCode,
                            test.TestName,
                            request.Reason,
                            request.UserID));
                    }
                    catch (Exception auditEx)
                    {
                        Console.WriteLine($"[AUDIT] AbortRunningTest logging failed for {request.SpecimenNo} / test {request.TestId}: {auditEx.Message}");
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