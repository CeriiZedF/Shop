using Shop.Models;

namespace Shop.DAL.Repository.IRepository
{
    public interface IProductUsageRepository : IRepository<ProductUsage>
    {
        void Update(ProductUsage productUsage);
    }
}
