using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class OnSiteSectionHeaderRepo : GenericRepo<OnSite_Section_Header>
    {
        public OnSiteSectionHeaderRepo(AppDbContext context) : base(context) { }

        public OnSite_Section_Header? GetBySpecimenAndSection(string specimenNo, string sectionCode)
            => dbSet.FirstOrDefault(h => h.SpecimenNo == specimenNo && h.SectionCode == sectionCode);

        public List<OnSite_Section_Header> GetBySectionCode(string sectionCode)
            => dbSet.Where(h => h.SectionCode == sectionCode).ToList();
    }
}