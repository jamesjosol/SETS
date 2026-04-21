using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class RunnerService : IRunnerService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public RunnerService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
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

                    return new ScanSpecimenResponse
                    {
                        HeaderId = header.Id,
                        SpecimenNo = header.SpecimenNo,
                        SectionCode = header.SectionCode,
                        TestGroupCode = header.TestGroupCode,
                        SampleTypeCode = header.SampleTypeCode,
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
                    var testIds = new HashSet<int>(request.Assignments.Select(a => a.TestId));

                    // Load all tests in memory first — avoids EF Core CTE syntax error with Contains()
                    var tests = context.Specimen_Section_Test
                        .ToList()
                        .Where(t => testIds.Contains(t.Id))
                        .ToList();

                    if (!tests.Any())
                        throw new Exception("No matching tests found.");

                    foreach (var assignment in request.Assignments)
                    {
                        var test = tests.FirstOrDefault(t => t.Id == assignment.TestId);
                        if (test == null) continue;

                        test.UpdatedBy = request.UserID;
                        test.Updated = now;

                        if (!string.IsNullOrEmpty(assignment.ScheduleTag))
                        {
                            switch (assignment.ScheduleTag)
                            {
                                case "ERD":
                                    test.ScheduleTag = "ERD";
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

                                case "SRD":
                                    test.ScheduleTag = "SRD";
                                    test.RunningDate = null;
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

                    context.SaveChanges();

                    // ── Save remarks per header ────────────────────────────────────
                    if (request.SpecimenRemarks != null && request.SpecimenRemarks.Any())
                    {
                        var headerIds = new HashSet<int>(request.SpecimenRemarks.Select(r => r.HeaderId));

                        var headers = context.Specimen_Section_Header
                            .ToList()
                            .Where(h => headerIds.Contains(h.Id))
                            .ToList();

                        foreach (var remarkItem in request.SpecimenRemarks)
                        {
                            var h = headers.FirstOrDefault(x => x.Id == remarkItem.HeaderId);
                            if (h == null) continue;

                            h.Remarks = remarkItem.Remarks;
                            h.Updated = now;
                            h.UpdatedBy = request.UserID;
                            context.Specimen_Section_Header.Update(h);
                        }

                        context.SaveChanges();
                    }

                    // ── Re-derive header status from all its children ──────────────
                    var headerId = tests.First().HeaderId;

                    var allTests = context.Specimen_Section_Test
                        .ToList()
                        .Where(t => t.HeaderId == headerId)
                        .ToList();

                    var header = context.Specimen_Section_Header
                        .FirstOrDefault(h => h.Id == headerId);

                    if (header != null)
                    {
                        if (allTests.All(t => t.Status == "X"))
                            header.Status = "C";
                        else if (allTests.Any(t => t.Status == "S") &&
                                 allTests.All(t => t.Status == "S" || t.Status == "X"))
                            header.Status = "S";
                        else
                            header.Status = "P";

                        header.Updated = now;
                        header.UpdatedBy = request.UserID;
                        context.Specimen_Section_Header.Update(header);
                        context.SaveChanges();
                    }

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
        public List<Specimen_Section_Header> GetPendingSpecimens(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                return context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode && h.Status == "P")
                    .OrderBy(h => h.Routed)
                    .ToList();
            }
            catch { throw; }
        }

        // ── Get saved specimens ────────────────────────────────────────────
        public List<Specimen_Section_Test> GetSavedTests(string sectionCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);

                var headers = context.Specimen_Section_Header
                    .Where(h => h.SectionCode == sectionCode)
                    .Select(h => h.Id)
                    .ToList();

                return context.Specimen_Section_Test
                    .Where(t => headers.Contains(t.HeaderId) && t.Status == "S")
                    .OrderBy(t => t.RunningDate)
                    .ToList();
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