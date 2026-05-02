using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class IssueIncidentTypeRepo : GenericRepo<Issue_IncidentType>
    {
        public IssueIncidentTypeRepo(AppDbContext context) : base(context) { }

        public List<Issue_IncidentType> GetAll()
            => dbSet.OrderBy(i => i.CreatedAt).ToList();

        public Issue_IncidentType? GetById(int id)
            => dbSet.FirstOrDefault(i => i.Id == id);
    }
}
