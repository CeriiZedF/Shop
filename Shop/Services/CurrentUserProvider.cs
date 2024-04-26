using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shop.DAL.Data;
using Shop.Models;

namespace Shop.Services
{
    // сервис для работы с пользователем, его ролями
    public class CurrentUserProvider
    {
        private readonly DataContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public CurrentUserProvider(
            DataContext db,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<ShopUser?> GetCurrentShopUser()
        {
            ClaimsPrincipal? user = _httpContextAccessor?.HttpContext?.User;
            if (user is null) { return null; }

            Claim? userId = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userId is null) { return null; }

            return await _db.ShopUsers.FirstOrDefaultAsync(u => u.Id == userId.Value);
        }

        public async Task<string?> GetUserRoles(ShopUser? user)
        {
            if (user is null) { return null; }

            string roles = "Admin";
            foreach (var role in await _userManager.GetRolesAsync(user))
            {
                roles += $"{role} ";
            }
            return roles.Trim();
        }
    }
}
