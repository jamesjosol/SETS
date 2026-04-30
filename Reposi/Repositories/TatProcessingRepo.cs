using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TatProcessingRepo : GenericRepo<Tat_Processing>
    {
        public TatProcessingRepo(AppDbContext context) : base(context) { }

        public Tat_Processing? Get(string branchCode)
            => dbSet.FirstOrDefault(t => t.BranchCode == branchCode);
    }
}