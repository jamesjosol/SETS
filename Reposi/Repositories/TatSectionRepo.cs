using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TatSectionRepo : GenericRepo<Tat_Section>
    {
        public TatSectionRepo(AppDbContext context) : base(context) { }

        public Tat_Section? GetBySection(string sectionCode)
            => dbSet.FirstOrDefault(t => t.SectionCode == sectionCode);

        public List<Tat_Section> GetAll()
            => dbSet.OrderBy(t => t.SectionCode).ToList();
    }
}