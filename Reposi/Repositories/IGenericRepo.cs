using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reposi.Repositories
{
    public interface IGenericRepo<T> where T : class
    {
        T? Get(object primaryKey);
        IEnumerable<T> GetAll();
        IQueryable<T> Query();
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Delete(object primaryKey);
    }
}
