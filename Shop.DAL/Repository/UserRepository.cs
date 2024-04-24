using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

using Shop.DAL.Repository.IRepository;
using Shop.DAL.Data;
using Shop.Models;

namespace Shop.DAL.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        internal DbSet<ShopUser> dbSet;

        public UserRepository(
            DataContext db,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
            dbSet = _db.Set<ShopUser>();
        }

        public async Task<ShopUser?> FirstOrDefault(
            Expression<Func<ShopUser, bool>>? filter = null,
            string? includeProperties = null,
            bool isTracking = false)
        {
            IQueryable<ShopUser> query = dbSet;
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

        public async Task<IEnumerable<ShopUser>> GetAll(
            Expression<Func<ShopUser, bool>>? filter = null,
            Func<IQueryable<ShopUser>,
            IOrderedQueryable<ShopUser>>? orderBy = null,
            string? includeProperties = null, bool isTracking = false)
        {
            IQueryable<ShopUser> query = dbSet;
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

        public async Task<ShopUser?> Find(string id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<bool> Add(ShopUser user, string password)
        {
            user.UserName = user.Email;
            var createUser = await _userManager.CreateAsync(user, password);
            return createUser.Succeeded;
        }

        public async Task AddRole(ShopUser user, string role, string? deleteRole = null)
        {
            if (deleteRole is not null)
            {
                await _userManager.RemoveFromRoleAsync(user, deleteRole);
            }
            await _userManager.AddToRoleAsync(user, role);
        }

        public async Task<string?> GetRole(ShopUser user)
        {
            if (user is null) { return null; }

            string roles = "";
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                roles += $"{role} ";
            }
            return roles.Trim();
        }

        public async Task<IList<Claim>> GetClaims(ShopUser user)
        {
            return await _userManager.GetClaimsAsync(user);
        }

        public async Task Remove(ShopUser user)
        {
            var identityUser = await Find(user.Id);
            if (identityUser is not null)
            {
                await _userManager.DeleteAsync(identityUser);
            }
        }

        public void Update(ShopUser user, ShopUser oldUser)
        {
            user.FullName = oldUser.FullName;
            user.AddressDelivery = oldUser.AddressDelivery;
            user.Email = oldUser.Email;
            _db.Update(user);
        }

        public async Task UpdateName(ShopUser user)
        {
            await _userManager.SetUserNameAsync(user, user.Email);
        }

        public async Task UpdatePassword(ShopUser user, string password)
        {
            string token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, password);
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }

        public SelectList? GetAllDropDownList(string obj, string? _selectedValue = null)
        {
            if (obj.Equals("Role"))
            {
                return new SelectList(new List<string>
                {
                    WC.CustomerRole,
                    WC.ManagerRole,
                    WC.AdminRole,
                }, selectedValue: _selectedValue);
            }
            return null;
        }
    }
}
