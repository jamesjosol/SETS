using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Reposi;
using Reposi.Context;
using System.Linq.Expressions;

namespace Reposi.Repositories
{
    public class GenericRepo<T> : IGenericRepo<T> where T : class
    {
        private readonly AppDbContext _context;
        internal DbSet<T> dbSet;

        public GenericRepo(AppDbContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }

        public T? Get(object primaryKey) => dbSet.Find(primaryKey);

        public IEnumerable<T> GetAll() => dbSet.ToList();

        public IQueryable<T> Query() => dbSet;

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate) => dbSet.Where(predicate);

        public void Add(T entity)
        {
            dbSet.Add(entity);
            _context.SaveChanges();
        }

        public void Update(T entity)
        {
            dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                dbSet.Attach(entity);
            dbSet.Remove(entity);
            _context.SaveChanges();
        }

        public void Delete(object primaryKey)
        {
            var entity = dbSet.Find(primaryKey);
            if (entity != null) Delete(entity);
        }
    }
}