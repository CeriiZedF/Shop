using System.Linq.Expressions;

namespace Shop.DAL.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAll(
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false
        );
        Task<T?> FirstOrDefault(
            Expression<Func<T, bool>>? filter = null,
            string? includeProperties = null,
            bool isTracking = false
        );
        Task<T?> Find(int id);
        Task Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
        Task Save();
    }
}
