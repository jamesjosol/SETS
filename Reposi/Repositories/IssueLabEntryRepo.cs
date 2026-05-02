using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class IssueLabEntryRepo : GenericRepo<Issue_LabEntry>
    {
        public IssueLabEntryRepo(AppDbContext context) : base(context) { }

        public List<Issue_LabEntry> GetBySubCategoryId(int subCategoryId)
            => dbSet.Where(e => e.SubCategoryId == subCategoryId)
                    .OrderByDescending(e => e.LoggedAt)
                    .ToList();

        public int CountBySubCategoryId(int subCategoryId)
            => dbSet.Count(e => e.SubCategoryId == subCategoryId);
    }
}