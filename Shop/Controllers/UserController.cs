using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using Shop.Services;

using Shop.Models;
using Shop.Models.ViewModels;
using Shop.DAL.Repository.IRepository;
using Microsoft.Extensions.Caching.Memory;

namespace Shop.Controllers
{
    [Authorize(Policy = WC.AdminPolicy)]
    public class UserController : Controller
    {
        private readonly CurrentUserProvider _currentUserProvider;
        private readonly IUserRepository _userRepository;
        private readonly IMemoryCache _memoryCache;

        public UserController(
            CurrentUserProvider currentUserProvider,
            IUserRepository userRepository,
            IMemoryCache memoryCache)
        {
            _currentUserProvider = currentUserProvider;
            _userRepository = userRepository;
            _memoryCache = memoryCache;
        }


        // Get User JWT Token
        public IActionResult GetToken()
        {
            return View();
        }


        // вывод данных из claims
        public async Task<IActionResult> GetClaimsData()
        {
            ShopUser? user = await _currentUserProvider.GetCurrentShopUser();
            IList<Claim>? claims = null;
            if (user is not null)
            {
                claims = await _userRepository.GetClaims(user);
            }
            return View(claims);
        }


        // READ
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAll();

            List<UserVM> usersVM = new();
            foreach (var user in users)
            {
                var role = await _currentUserProvider.GetUserRoles(user) ?? "-";
                usersVM.Add(new UserVM { ShopUser = user, Role = role });
            }
            return View(usersVM);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userRepository.GetAll();

            List<UserVM> usersVM = new();
            foreach (var user in users)
            {
                var role = await _currentUserProvider.GetUserRoles(user) ?? "-";
                usersVM.Add(new UserVM { ShopUser = user, Role = role });
            }

            return Json(new { data = usersVM });
        }


        // CREATE
        public IActionResult Create()
        {
            return View(new UserVM()
            {
                Roles = _userRepository.GetAllDropDownList("Role")
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserVM? userVM)
        {
            // проверка пароля
            if (!ModelState.IsValid || userVM is null ||
                userVM?.Password?.Length < 3 || userVM?.Password?.Length > 25)
            {
                return RedirectToAction(nameof(Create));
            }

            // создаём пользователя
            if (!await _userRepository.Add(userVM.ShopUser, userVM.Password))
            {
                return RedirectToAction(nameof(Create));
            }

            if (userVM.Role is not null)
            {
                await _userRepository.AddRole(userVM.ShopUser, userVM.Role);
            }

            return RedirectToAction(nameof(Index));
        }


        // EDIT
        public async Task<IActionResult> Edit(string? id)
        {
            if (id is null) { return NotFound(); }

            var user = await _userRepository.FirstOrDefault(u => u.Id == id);
            if (user is null) { return NotFound(); }

            string role = await _currentUserProvider.GetUserRoles(user) ?? "";
            UserVM createUserVM = new()
            {
                ShopUser = user,
                Roles = _userRepository.GetAllDropDownList("Role", role),
                Role = role
            };

            return View(createUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserVM? createUserVM)
        {
            if (!ModelState.IsValid || createUserVM is null)
            {
                return RedirectToAction(nameof(Create));
            }

            var user = await _userRepository.FirstOrDefault(u => u.Id == createUserVM.ShopUser.Id);
            if (user is null) { return NotFound(); }

            // обновление свойств пользователя
            _userRepository.Update(user, createUserVM.ShopUser);

            // если изменили роль, то устанавливаем
            string? userOldRole = await _currentUserProvider.GetUserRoles(user);
            if (createUserVM.Role is not null && !createUserVM.Role.Equals(userOldRole))
            {
                await _userRepository.AddRole(user, createUserVM.Role, userOldRole);
            }

            // если email(UserName) изменен, то устанавливаем новый (email совпадает с именем)
            if (!user.UserName.Equals(user.Email))
            {
                await _userRepository.UpdateName(user);
            }

            // если пароль изменен, устанавливаем новый
            if (!String.IsNullOrEmpty(createUserVM.Password))
            {
                await _userRepository.UpdatePassword(user, createUserVM.Password);
            }

            await _userRepository.Save();
            return RedirectToAction(nameof(Index));
        }


        // DELETE
        public async Task<IActionResult> Delete(string? id)
        {
            if (id is null) { return NotFound(); }

            var user = await _userRepository.FirstOrDefault(u => u.Id == id);
            if (user is null) { return NotFound(); }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(ShopUser? user)
        {
            if (user is not null) { await _userRepository.Remove(user); }
            return RedirectToAction(nameof(Index));
        }
    }
}
