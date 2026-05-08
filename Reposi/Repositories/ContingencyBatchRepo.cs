using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class ContingencyBatchRepo : GenericRepo<Contingency_Batch>
    {
        public ContingencyBatchRepo(AppDbContext context) : base(context) { }

        public Contingency_Batch? GetByBatchNo(string batchNo)
            => Query().FirstOrDefault(b => b.BatchNo == batchNo);

        public List<Contingency_Batch> GetByEndorsedTo(string branchCode)
            => Query().Where(b => b.EndorsedTo == branchCode).OrderByDescending(b => b.EndorsedAt).ToList();

        public string GenerateNextBatchNo(string branchCode)
        {
            var today = DateTime.Now;
            var prefix = $"CTG-{branchCode}-{today:yyyyMMdd}-";
            var lastBatch = Query()
                .Where(b => b.BatchNo.StartsWith(prefix))
                .OrderByDescending(b => b.BatchNo)
                .FirstOrDefault();

            int seq = 1;
            if (lastBatch != null)
            {
                var parts = lastBatch.BatchNo.Split('-');
                if (parts.Length == 4 && int.TryParse(parts[3], out int last))
                    seq = last + 1;
            }
            return $"{prefix}{seq:D3}";
        }
    }
}