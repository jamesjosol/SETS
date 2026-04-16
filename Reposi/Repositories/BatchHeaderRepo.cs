using Model.SETSDB;
using Reposi.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposi.Repositories
{
    public class BatchHeaderRepo : GenericRepo<Batch_Header>
    {
        public BatchHeaderRepo(AppDbContext context) : base(context) { }

        public Batch_Header? GetByBatchNo(string batchNo)
            => dbSet.FirstOrDefault(b => b.BatchNo == batchNo);

        public List<Batch_Header> GetByLocation(string location)
            => dbSet.Where(b => b.Location == location).ToList();
    }
}
