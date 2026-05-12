using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class BranchSettingsRepo : GenericRepo<Branch_Settings>
    {
        public BranchSettingsRepo(AppDbContext context) : base(context) { }

        public Branch_Settings Get()
            => dbSet.FirstOrDefault(s => s.Id == 1)
               ?? new Branch_Settings { Id = 1, IsOutboundEnabled = false };
    }
}