// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shop.Areas.API;
using Shop.DAL.Repository.IRepository;

namespace Shop.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<LoginModel> _logger;
        private readonly JWTSettings _settingsJWT;
        private readonly IMemoryCache _memoryCache;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            IUserRepository userRepository,
            ILogger<LoginModel> logger,
            IOptions<JWTSettings> settingsJWT,
            IMemoryCache memoryCache)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
            _settingsJWT = settingsJWT.Value;
            _memoryCache = memoryCache;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/Home/Index");

            // очищаем сущ. внешний файл cookie, чтобы обеспечить чистый процесс входа в систему
            //await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Home/Index");  // преобразование относительного в абсолютный путь

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // lockoutOnFailure: false - не учитывает ошибки входа в систему для блокировки учетной записи
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");

                    //////////// JWT token //////////////
                    var user = await _userRepository.FirstOrDefault(u => u.Email.Equals(Input.Email), isTracking: true);
                    var role = await _userRepository.GetRole(user);
                    
                    var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settingsJWT.SecretKey));
                    List<Claim> claims = new()
                    {
                        new Claim(ClaimTypes.Email, Input.Email),
                        new Claim(ClaimTypes.Role, role),
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.GivenName, user.FullName),
                        new Claim(ClaimTypes.StreetAddress, user.AddressDelivery),
                    };
                    var jwt = new JwtSecurityToken(
                         issuer: _settingsJWT.Issure,
                         audience: _settingsJWT.Audience,
                         claims: claims,
                         expires: DateTime.Now.AddMinutes(10),
                         signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
                    );
                    _memoryCache.Set("JwtToken", new JwtSecurityTokenHandler().WriteToken(jwt));
                    /////////////////////////////////////

                    //////////// Claims //////////////
                    var existingClaims = await _userManager.GetClaimsAsync(user);
                    if (existingClaims is not null && existingClaims.Any())
                    {
                        var res = await _userManager.RemoveClaimsAsync(user, existingClaims);
                        if (res.Succeeded)
                        {
                            await _userManager.AddClaimsAsync(user, claims);
                        }
                    }
                    else
                    {
                        await _userManager.AddClaimsAsync(user, claims);
                    }
                    /////////////////////////////////////

                    return LocalRedirect(returnUrl);
                }
                else if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
            return Page();
        }
    }
}
