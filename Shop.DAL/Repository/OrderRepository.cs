using Shop.DAL.Data;
using Shop.DAL.Repository.IRepository;
using Shop.Models;

namespace Shop.DAL.Repository
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly DataContext _db;

        public OrderRepository(DataContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Order order)
        {
            _db.Update(order);
        }
    }
}
