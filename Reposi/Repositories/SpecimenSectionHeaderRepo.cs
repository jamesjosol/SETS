using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class SpecimenSectionHeaderRepo : GenericRepo<Specimen_Section_Header>
    {
        public SpecimenSectionHeaderRepo(AppDbContext context) : base(context) { }

        public List<Specimen_Section_Header> GetBySectionCode(string sectionCode)
            => dbSet.Where(h => h.SectionCode == sectionCode).ToList();

        public List<Specimen_Section_Header> GetPendingBySection(string sectionCode)
            => dbSet.Where(h => h.SectionCode == sectionCode && h.Status == "P").ToList();

        public List<Specimen_Section_Header> GetSavedBySection(string sectionCode)
            => dbSet.Where(h => h.SectionCode == sectionCode && h.Status == "S").ToList();

        public Specimen_Section_Header? GetBySpecimenAndGroup(string specimenNo, string testGroupCode)
            => dbSet.FirstOrDefault(h => h.SpecimenNo == specimenNo && h.TestGroupCode == testGroupCode);

        public List<Specimen_Section_Header> GetBySpecimenNo(string specimenNo)
            => dbSet.Where(h => h.SpecimenNo == specimenNo).ToList();

        public bool Exists(string specimenNo, string testGroupCode)
            => dbSet.Any(h => h.SpecimenNo == specimenNo && h.TestGroupCode == testGroupCode);
    }
}