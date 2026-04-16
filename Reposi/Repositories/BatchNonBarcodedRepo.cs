using Model.SETSDB;
using Reposi.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposi.Repositories
{
    public class BatchNonBarcodedRepo : GenericRepo<Batch_NonBarcoded>
    {
        public BatchNonBarcodedRepo(AppDbContext context) : base(context) { }

        public List<Batch_NonBarcoded> GetByBatchNo(string batchNo)
            => dbSet.Where(b => b.BatchNo == batchNo).ToList();
    }
}
