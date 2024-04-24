using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shop.Models;

namespace Shop.DAL.Data
{
    public class DataContext : IdentityDbContext
    {
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductUsage> ProductsUsage { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<ShopUser> ShopUsers { get; set; } = null!;

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
    }
}
