using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class ContingencySpecimenRepo : GenericRepo<Contingency_Specimen>
    {
        public ContingencySpecimenRepo(AppDbContext context) : base(context) { }

        public List<Contingency_Specimen> GetByBatchId(int batchId)
            => Query().Where(s => s.BatchId == batchId).ToList();

        public Contingency_Specimen? GetByBatchAndSpecimenNo(int batchId, string specimenNo)
            => Query().FirstOrDefault(s => s.BatchId == batchId && s.SpecimenNo == specimenNo);

        public int CountReceived(int batchId)
            => Query().Count(s => s.BatchId == batchId && s.IsReceived);

        public int CountTotal(int batchId)
            => Query().Count(s => s.BatchId == batchId);
    }
}