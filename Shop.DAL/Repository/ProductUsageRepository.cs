using Shop.Models;
using Shop.DAL.Repository.IRepository;
using Shop.DAL.Data;

namespace Shop.DAL.Repository
{
    public class ProductUsageRepository : Repository<ProductUsage>, IProductUsageRepository
    {
        private readonly DataContext _db;

        public ProductUsageRepository(DataContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductUsage productUsage)
        {
            _db.Update(productUsage);
        }
    }
}
