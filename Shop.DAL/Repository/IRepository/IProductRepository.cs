using Microsoft.AspNetCore.Mvc.Rendering;
using Shop.Models;

namespace Shop.DAL.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
        SelectList? GetAllDropDownList(string obj);
    }
}
