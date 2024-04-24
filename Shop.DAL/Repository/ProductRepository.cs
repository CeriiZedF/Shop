using Microsoft.AspNetCore.Mvc.Rendering;

using Shop.DAL.Repository.IRepository;
using Shop.DAL.Data;
using Shop.Models;

namespace Shop.DAL.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly DataContext _db;

        public ProductRepository(DataContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
        }

        public SelectList? GetAllDropDownList(string obj)
        {
            if (obj.Equals("Category"))
            {
                return new SelectList(_db.Categories, "Id", "Name");
            }
            else if (obj.Equals("ProductUsage"))
            {
                return new SelectList(_db.ProductsUsage, "Id", "Name");
            }
            return null;
        }
    }
}
