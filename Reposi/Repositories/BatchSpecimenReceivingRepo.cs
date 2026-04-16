using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class BatchSpecimenReceivingRepo : GenericRepo<Batch_Specimen_Receiving>
    {
        public BatchSpecimenReceivingRepo(AppDbContext context) : base(context) { }

        public Batch_Specimen_Receiving? GetBySpecimenAndBatch(string specimenNo, string batchNo)
            => dbSet.FirstOrDefault(r => r.SpecimenNo == specimenNo && r.BatchNo == batchNo);

        public List<Batch_Specimen_Receiving> GetByBatchNo(string batchNo)
            => dbSet.Where(r => r.BatchNo == batchNo).ToList();

        public bool IsReceived(string specimenNo, string batchNo)
            => dbSet.Any(r => r.SpecimenNo == specimenNo && r.BatchNo == batchNo);
    }
}