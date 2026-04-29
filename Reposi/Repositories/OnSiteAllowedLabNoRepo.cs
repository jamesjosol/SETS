using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class OnSiteAllowedLabNoRepo : GenericRepo<OnSite_AllowedLabNo>
    {
        public OnSiteAllowedLabNoRepo(AppDbContext context) : base(context) { }

        public List<OnSite_AllowedLabNo> GetAllActive()
            => dbSet.Where(e => e.Active).OrderBy(e => e.Prefix).ToList();

        public OnSite_AllowedLabNo? GetByPrefix(string prefix)
            => dbSet.FirstOrDefault(e => e.Prefix == prefix);
    }
}