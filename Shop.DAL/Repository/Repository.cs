using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

using Shop.DAL.Repository.IRepository;
using Shop.DAL.Data;

namespace Shop.DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly DataContext _db;
        internal DbSet<T> dbSet;

        public Repository(DataContext db)
        {
            _db = db;
            this.dbSet = _db.Set<T>();
        }

        public async Task<T?> FirstOrDefault(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool isTracking = false)
        {
            IQueryable<T> query = dbSet;
            if (filter is not null)
            {
                query = query.Where(filter);
            }
            if (includeProperties is not null)
            {
                foreach (string property in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            if (!isTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<T>> GetAll(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false)
        {
            IQueryable<T> query = dbSet;
            if (filter is not null)
            {
                query = query.Where(filter);
            }
            if (includeProperties is not null)
            {
                foreach (var property in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            if (orderBy is not null)
            {
                query = orderBy(query);
            }
            if (!isTracking)
            {
                query = query.AsNoTracking();
            }
            return await query.ToListAsync();
        }

        public async Task<T?> Find(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task Add(T entity)
        {
            await dbSet.AddAsync(entity);
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }
    }
}
