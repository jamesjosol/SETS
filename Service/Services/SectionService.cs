using Model.SETSDB;
using Reposi;
using Reposi.Context;
using Service.Interfaces;

namespace Service.Services
{
    public class SectionService : ISectionService
    {
        private readonly AppDbContextFactory _factory;
        private readonly string _branch;

        public SectionService(AppDbContextFactory factory, string branch)
        {
            _factory = factory;
            _branch = SetsConnection.ConnectionString(branch);
        }

        public List<Section_Master> GetAll()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetAll().ToList();
            }
            catch { throw; }
        }

        public List<Section_Master> GetActive()
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetActive();
            }
            catch { throw; }
        }

        public Section_Master? GetByCode(string code)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetByCode(code);
            }
            catch { throw; }
        }

        public List<Section_Master> GetByBranch(string branchCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetByBranch(branchCode);
            }
            catch { throw; }
        }

        public void Add(Section_Master section)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Sections.Add(section);
            }
            catch { throw; }
        }

        public void Update(Section_Master section)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                unit.Sections.Update(section);
            }
            catch { throw; }
        }

        // ── External partner section methods ──────────────────────────────

        public List<Section_Master> GetByBranchIncludeInactive(string branchCode)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);
                return unit.Sections.GetByBranchIncludeInactive(branchCode);
            }
            catch { throw; }
        }

        public Section_Master? LookupFromRemote(string partnerBranchCode, string sectionCode)
        {
            try
            {
                var remoteConn = SetsConnection.ConnectionString(partnerBranchCode);
                using var remoteContext = _factory.CreateContext(remoteConn);
                return remoteContext.Section_Master
                    .FirstOrDefault(s => s.Code == sectionCode && s.Active);
            }
            catch { throw; }
        }

        public void AddForeignSection(string partnerBranchCode, string sectionCode,
            string sectionName, string category, string createdBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var existing = unit.Sections.GetByCodeAndBranch(sectionCode, partnerBranchCode);

                if (existing != null)
                {
                    if (existing.Active)
                        throw new Exception($"Section '{sectionCode}' is already registered for branch '{partnerBranchCode}'.");

                    existing.Active = true;
                    existing.Name = sectionName;
                    existing.Category = category;
                    existing.Updated = DateTime.Now;
                    existing.UpdatedBy = createdBy;
                    unit.Sections.Update(existing);
                    return;
                }

                unit.Sections.Add(new Section_Master
                {
                    Code = sectionCode,
                    Name = sectionName,
                    BranchCode = partnerBranchCode,
                    Category = category,
                    Active = true,
                    AutoNo = 0,
                    Created = DateTime.Now,
                    CreatedBy = createdBy
                });
            }
            catch { throw; }
        }

        public void RemoveForeignSection(string partnerBranchCode, string sectionCode, string updatedBy)
        {
            try
            {
                using var context = _factory.CreateContext(_branch);
                using var unit = new UnitOfWork(context);

                var section = unit.Sections.GetByCodeAndBranch(sectionCode, partnerBranchCode)
                    ?? throw new Exception($"Section '{sectionCode}' not found for branch '{partnerBranchCode}'.");

                section.Active = false;
                section.Updated = DateTime.Now;
                section.UpdatedBy = updatedBy;
                unit.Sections.Update(section);
            }
            catch { throw; }
        }
    }
}