using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

using Shop.Models;

namespace Shop.DAL.Repository.IRepository
{
    public interface IUserRepository
    {
        Task<IEnumerable<ShopUser>> GetAll(
            Expression<Func<ShopUser, bool>>? filter = null,
            Func<IQueryable<ShopUser>, IOrderedQueryable<ShopUser>>? orderBy = null,
            string? includeProperties = null,
            bool isTracking = false
        );
        Task<ShopUser?> FirstOrDefault(
            Expression<Func<ShopUser, bool>>? filter = null,
            string? includeProperties = null,
            bool isTracking = false
        );
        Task<ShopUser?> Find(string id);
        Task<bool> Add(ShopUser entity, string password);
        Task AddRole(ShopUser user, string role, string? deleteRole = null);
        Task<string?> GetRole(ShopUser user);
        Task<IList<Claim>> GetClaims(ShopUser user);
        Task Remove(ShopUser entity);
        void Update(ShopUser user, ShopUser oldUser);
        Task UpdateName(ShopUser user);
        Task UpdatePassword(ShopUser user, string password);
        Task Save();
        public SelectList? GetAllDropDownList(string obj, string? _selectedValue = null);
    }
}
