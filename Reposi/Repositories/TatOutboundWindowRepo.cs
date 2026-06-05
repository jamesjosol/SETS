using System;
using System.Collections.Generic;
using System.Linq;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class TatOutboundWindowRepo : GenericRepo<Tat_Outbound_Window>
    {
        public TatOutboundWindowRepo(AppDbContext context) : base(context) { }

        public List<Tat_Outbound_Window> GetAll()
            => dbSet.OrderBy(w => w.ScheduleType)
                    .ThenBy(w => w.WindowStart)
                    .ToList();

        public List<Tat_Outbound_Window> GetActive()
            => dbSet.Where(w => w.IsActive)
                    .OrderBy(w => w.ScheduleType)
                    .ThenBy(w => w.WindowStart)
                    .ToList();

        public Tat_Outbound_Window? GetById(int id)
            => dbSet.FirstOrDefault(w => w.Id == id);
    }
}