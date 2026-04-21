using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class SectionTestGroupRepo : GenericRepo<Section_TestGroup>
    {
        public SectionTestGroupRepo(AppDbContext context) : base(context) { }

        public List<Section_TestGroup> GetBySectionCode(string sectionCode)
            => dbSet.Where(s => s.SectionCode == sectionCode).ToList();

        public List<Section_TestGroup> GetActive()
            => dbSet.Where(s => s.Active).ToList();

        public Section_TestGroup? GetBySectionAndGroup(string sectionCode, string testGroupCode)
            => dbSet.FirstOrDefault(s => s.SectionCode == sectionCode && s.TestGroupCode == testGroupCode);

        // Given a list of OD_TEST_GRP values, return the distinct section codes that own them
        public List<string> ResolveSectionCodes(List<string> testGroupCodes)
            => dbSet
                .Where(s => s.Active && testGroupCodes.Contains(s.TestGroupCode))
                .Select(s => s.SectionCode)
                .Distinct()
                .ToList();
    }
}