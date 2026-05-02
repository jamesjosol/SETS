using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class IssueCommentRepo : GenericRepo<Issue_Comment>
    {
        public IssueCommentRepo(AppDbContext context) : base(context) { }

        public List<Issue_Comment> GetByIncidentTypeId(int incidentTypeId)
            => dbSet.Where(c => c.IncidentTypeId == incidentTypeId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

        public Issue_Comment? GetById(int id)
            => dbSet.FirstOrDefault(c => c.Id == id);
    }
}