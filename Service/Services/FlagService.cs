using Model.Main;
using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class FlagService : IFlagService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;
        private readonly string _branch_raw;

        public FlagService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
            _branch_raw = branch;
        }

        public FlagSpecimenResult FlagSpecimen(FlagSpecimenRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var specimen = unit.BatchSpecimens
                    .FindBy(s => s.SpecimenNo == request.SpecimenNo
                              && s.BatchNo == request.BatchNo)
                    .FirstOrDefault()
                    ?? throw new Exception($"Specimen '{request.SpecimenNo}' not found in batch '{request.BatchNo}'.");

                if (specimen.Status == "R")
                    throw new Exception("Specimen has already been received and cannot be flagged.");

                if (specimen.Status == "X")
                    throw new Exception("Specimen has been cancelled and cannot be flagged.");

                if (specimen.IsFlagged)
                    throw new Exception("Specimen is already flagged.");

                var header = unit.BatchHeaders
                    .FindBy(h => h.BatchNo == request.BatchNo)
                    .FirstOrDefault()
                    ?? throw new Exception($"Batch '{request.BatchNo}' not found.");

                specimen.IsFlagged = true;
                specimen.FlagReason = request.FlagReason.Trim();
                specimen.FlaggedBy = request.UserID;
                specimen.FlaggedAt = DateTime.Now;

                unit.BatchSpecimens.Update(specimen);

                // ── Mirror flag to destination branch if outbound ─────────────────
                if (header.IsOutbound && !string.IsNullOrEmpty(header.DestBranchCode))
                {
                    try
                    {
                        var destConn = SetsConnection.ConnectionString(header.DestBranchCode);
                        using var destContext = _factory.CreateContext(destConn);
                        using var destUnit = new UnitOfWork(destContext);

                        var destSpecimen = destUnit.BatchSpecimens
                            .FindBy(s => s.SpecimenNo == request.SpecimenNo
                                      && s.BatchNo == request.BatchNo)
                            .FirstOrDefault();

                        if (destSpecimen != null && destSpecimen.Status == "P")
                        {
                            destSpecimen.IsFlagged = true;
                            destSpecimen.FlagReason = request.FlagReason.Trim();
                            destSpecimen.FlaggedBy = request.UserID;
                            destSpecimen.FlaggedAt = specimen.FlaggedAt;

                            destUnit.BatchSpecimens.Update(destSpecimen);
                        }
                    }
                    catch (Exception destEx)
                    {
                        Console.WriteLine($"[FLAG] Mirror flag to {header.DestBranchCode} failed for {request.SpecimenNo}: {destEx.Message}");
                    }
                }

                return new FlagSpecimenResult
                {
                    ProcDestination = header.ProcDestination,
                    LocationName = header.Location,
                    PatientName = specimen.PatientName,
                    PID = specimen.PID
                };
            }
            catch { throw; }
        }

        public FlagSpecimenResult UnflagSpecimen(UnflagSpecimenRequest request)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var specimen = unit.BatchSpecimens
                    .FindBy(s => s.SpecimenNo == request.SpecimenNo
                              && s.BatchNo == request.BatchNo)
                    .FirstOrDefault()
                    ?? throw new Exception($"Specimen '{request.SpecimenNo}' not found in batch '{request.BatchNo}'.");

                if (specimen.Status == "R")
                    throw new Exception("Specimen has already been received and cannot be unflagged.");

                if (specimen.Status == "X")
                    throw new Exception("Specimen has been cancelled and cannot be unflagged.");

                if (!specimen.IsFlagged)
                    throw new Exception("Specimen is not flagged.");

                var header = unit.BatchHeaders
                    .FindBy(h => h.BatchNo == request.BatchNo)
                    .FirstOrDefault()
                    ?? throw new Exception($"Batch '{request.BatchNo}' not found.");

                specimen.IsFlagged = false;
                specimen.FlagReason = null;
                specimen.FlaggedBy = null;
                specimen.FlaggedAt = null;

                unit.BatchSpecimens.Update(specimen);

                // ── Mirror unflag to destination branch if outbound ───────────────
                if (header.IsOutbound && !string.IsNullOrEmpty(header.DestBranchCode))
                {
                    try
                    {
                        var destConn = SetsConnection.ConnectionString(header.DestBranchCode);
                        using var destContext = _factory.CreateContext(destConn);
                        using var destUnit = new UnitOfWork(destContext);

                        var destSpecimen = destUnit.BatchSpecimens
                            .FindBy(s => s.SpecimenNo == request.SpecimenNo
                                      && s.BatchNo == request.BatchNo)
                            .FirstOrDefault();

                        if (destSpecimen != null && destSpecimen.Status == "P")
                        {
                            destSpecimen.IsFlagged = false;
                            destSpecimen.FlagReason = null;
                            destSpecimen.FlaggedBy = null;
                            destSpecimen.FlaggedAt = null;

                            destUnit.BatchSpecimens.Update(destSpecimen);
                        }
                    }
                    catch (Exception destEx)
                    {
                        Console.WriteLine($"[FLAG] Mirror unflag to {header.DestBranchCode} failed for {request.SpecimenNo}: {destEx.Message}");
                    }
                }

                return new FlagSpecimenResult
                {
                    ProcDestination = header.ProcDestination,
                    LocationName = header.Location,
                    PatientName = specimen.PatientName,
                    PID = specimen.PID
                };
            }
            catch { throw; }
        }
    }
}