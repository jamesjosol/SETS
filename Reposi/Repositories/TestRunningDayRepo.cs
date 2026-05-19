using System.Collections.Generic;
using System.Linq;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TestRunningDayRepo : GenericRepo<Test_RunningDay>
    {
        public TestRunningDayRepo(AppDbContext context) : base(context) { }

        public Test_RunningDay? GetByTestCode(string testCode)
            => dbSet.FirstOrDefault(t => t.TestCode == testCode);

        public List<Test_RunningDay> GetAll()
            => dbSet.OrderBy(t => t.TestName).ToList();

        /// <summary>
        /// Returns only entries whose TestGroupCode is in the given set.
        /// Used to scope the list for lab section TLs.
        /// </summary>
        public List<Test_RunningDay> GetByTestGroupCodes(List<string> testGroupCodes)
        {
            // Materialise first to avoid EF Core Contains() CTE issues
            var all = dbSet.OrderBy(t => t.TestName).ToList();
            return all
                .Where(t => t.TestGroupCode != null && testGroupCodes.Contains(t.TestGroupCode))
                .ToList();
        }
    }
}