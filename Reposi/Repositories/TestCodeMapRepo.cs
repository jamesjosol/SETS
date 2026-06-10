using System;
using System.Collections.Generic;
using System.Linq;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TestCodeMapRepo : GenericRepo<Test_Code_Map>
    {
        public TestCodeMapRepo(AppDbContext context) : base(context) { }

        public List<Test_Code_Map> GetAll()
            => Query().OrderByDescending(m => m.Created).ToList();

        public List<Test_Code_Map> GetActive()
            => Query().Where(m => m.IsActive).ToList();

        /// <summary>
        /// Resolves a test code to its mapped counterpart (two-way).
        /// If an active mapping exists where CodeA or CodeB matches,
        /// returns the other side. Otherwise returns the original code.
        /// </summary>
        public string ResolveCode(string code)
        {
            if (string.IsNullOrEmpty(code)) return code;

            var map = Query()
                .FirstOrDefault(m => m.IsActive &&
                    (m.CodeA == code || m.CodeB == code));

            if (map == null) return code;

            return map.CodeA == code ? map.CodeB : map.CodeA;
        }
    }
}