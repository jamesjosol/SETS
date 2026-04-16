using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class RefCodesRepo : GenericRepo<Ref_Codes>
    {
        public RefCodesRepo(AppDbContext context) : base(context) { }

        public List<Ref_Codes> GetByType(string type)
            => dbSet.Where(r => r.Type == type)
                    .OrderBy(r => r.DispSeq)
                    .ToList();

        public Ref_Codes? GetByTypeAndCode(string type, string code)
            => dbSet.FirstOrDefault(r => r.Type == type && r.Code == code);
    }
}
