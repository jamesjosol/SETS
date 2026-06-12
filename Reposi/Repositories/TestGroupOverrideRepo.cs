using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TestGroupOverrideRepo : GenericRepo<Test_Group_Override>
    {
        public TestGroupOverrideRepo(AppDbContext context) : base(context) { }

        public List<Test_Group_Override> GetAll()
            => Query().OrderByDescending(m => m.Created).ToList();

        public List<Test_Group_Override> GetActive()
            => Query().Where(m => m.IsActive).ToList();

        /// <summary>
        /// Returns the overridden test group for a given test code if an active mapping exists.
        /// Returns null if no override is found.
        /// </summary>
        public string? ResolveGroup(string testCode)
        {
            if (string.IsNullOrEmpty(testCode)) return null;

            var match = Query()
                .FirstOrDefault(m => m.IsActive && m.TestCode == testCode);

            return match?.OverrideGroup;
        }
    }
}