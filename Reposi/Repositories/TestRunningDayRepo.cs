using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
