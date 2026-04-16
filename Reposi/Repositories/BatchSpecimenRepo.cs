using Model.SETSDB;
using Reposi.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reposi.Repositories
{
    public class BatchSpecimenRepo : GenericRepo<Batch_Specimen>
    {
        public BatchSpecimenRepo(AppDbContext context) : base(context) { }

        public List<Batch_Specimen> GetByBatchNo(string batchNo)
            => dbSet.Where(b => b.BatchNo == batchNo).ToList();

        public Batch_Specimen? GetBySpecimenNo(string specimenNo)
            => dbSet.FirstOrDefault(b => b.SpecimenNo == specimenNo);
    }
}
