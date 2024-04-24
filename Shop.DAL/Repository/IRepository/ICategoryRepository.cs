using Shop.Models;

namespace Shop.DAL.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task Update(Category category);
    }
}
