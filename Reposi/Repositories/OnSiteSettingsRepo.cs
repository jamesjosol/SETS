using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class OnSiteSettingsRepo : GenericRepo<OnSite_Settings>
    {
        public OnSiteSettingsRepo(AppDbContext context) : base(context) { }

        public OnSite_Settings? GetSettings()
            => dbSet.FirstOrDefault();
    }
}