using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TestGroupRepo : GenericRepo<Test_Group>
    {
        public TestGroupRepo(AppDbContext context) : base(context) { }

        public Test_Group? GetByCode(string code)
            => dbSet.FirstOrDefault(t => t.Code == code);
    }
}