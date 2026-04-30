using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class ProcessingOptionsRepo : GenericRepo<Processing_Options>
    {
        public ProcessingOptionsRepo(AppDbContext context) : base(context) { }

        public Processing_Options? Get(string branchCode)
            => dbSet.FirstOrDefault(p => p.BranchCode == branchCode);
    }
}