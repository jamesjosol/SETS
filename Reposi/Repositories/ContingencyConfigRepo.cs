using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class ContingencyConfigRepo : GenericRepo<Contingency_Config>
    {
        public ContingencyConfigRepo(AppDbContext context) : base(context) { }

        public Contingency_Config? GetConfig() => Query().FirstOrDefault();
    }
}