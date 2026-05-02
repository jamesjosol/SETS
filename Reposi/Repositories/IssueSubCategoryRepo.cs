using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class IssueSubCategoryRepo : GenericRepo<Issue_SubCategory>
    {
        public IssueSubCategoryRepo(AppDbContext context) : base(context) { }

        public List<Issue_SubCategory> GetByIncidentTypeId(int incidentTypeId)
            => dbSet.Where(s => s.IncidentTypeId == incidentTypeId)
                    .OrderBy(s => s.CreatedAt)
                    .ToList();

        public Issue_SubCategory? GetById(int id)
            => dbSet.FirstOrDefault(s => s.Id == id);
    }
}