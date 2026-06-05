using System;
using System.Collections.Generic;
using System.Linq;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TatOutboundLogRepo : GenericRepo<Tat_Outbound_Log>
    {
        public TatOutboundLogRepo(AppDbContext context) : base(context) { }

        public Tat_Outbound_Log? GetByWindowAndDate(int windowId, DateTime windowDate)
            => dbSet.FirstOrDefault(l => l.WindowId == windowId
                                      && l.WindowDate == windowDate.Date);

        public Tat_Outbound_Log? GetById(int id)
            => dbSet.FirstOrDefault(l => l.Id == id);

        public List<Tat_Outbound_Log> GetByDateRange(DateTime dateFrom, DateTime dateTo)
            => dbSet.ToList()
                    .Where(l => l.WindowDate >= dateFrom.Date
                             && l.WindowDate <= dateTo.Date)
                    .OrderBy(l => l.WindowDate)
                    .ThenBy(l => l.WindowStart)
                    .ToList();
    }
}