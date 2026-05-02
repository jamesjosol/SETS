using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model.SETSDB;
using Reposi.Context;

namespace Reposi.Repositories
{
    public class IssueTagRepo : GenericRepo<Issue_Tag>
    {
        public IssueTagRepo(AppDbContext context) : base(context) { }

        public List<Issue_Tag> GetBySubCategoryId(int subCategoryId)
            => dbSet.Where(t => t.SubCategoryId == subCategoryId)
                    .OrderBy(t => t.CreatedAt)
                    .ToList();

        public List<string> GetAllTagNames()
            => dbSet.Select(t => t.TagName)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();

        public bool Exists(int subCategoryId, string tagName)
            => dbSet.Any(t => t.SubCategoryId == subCategoryId && t.TagName == tagName);
    }
}