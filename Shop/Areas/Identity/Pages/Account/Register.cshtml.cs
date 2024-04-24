// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Shop.Models;


namespace Shop.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        // модель для ввода данных, связанных с регистрацией пользователя
        [BindProperty]
        public InputModel Input { get; set; }

        // строка, содержит URL-адрес, на который польз. будет перенаправлен после успешной
        // аутентификации или регистрации. Например для перенаправления польз. обратно на страницу,
        // которую он запрашивал до процесса аутентификации
        public string ReturnUrl { get; set; }

        // список внешних поставщиков аутентификации (Google, Facebook, Twitter и тд)
        public IList<AuthenticationScheme> ExternalLogins { get; set; }


        // внутрений класс
        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Full name")]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "Adress delivery")]
            public string AddressDelivery { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            // получения списка внешних схем аутентификации
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // returnUrl - URL-адрес, на который польз. будет перенаправлен после выполнения операции
            returnUrl ??= Url.Content("~/Home/Index");  // преобразование относительного в абсолютный путь
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            if (ModelState.IsValid)
            {
                // создаём пользователя
                var user = new ShopUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    AddressDelivery = Input.AddressDelivery,
                    FullName = Input.FullName,
                };
                IdentityResult result = await _userManager.CreateAsync(user, Input.Password);

                // настраиваем роли
                if (user.Email == WC.NameUserAdmin)
                    await _userManager.AddToRoleAsync(user, WC.AdminRole);
                else if (user.Email == WC.NameUserManager)
                    await _userManager.AddToRoleAsync(user, WC.ManagerRole);
                else
                    await _userManager.AddToRoleAsync(user, WC.CustomerRole);

                //if (User.IsInRole(WC.AdminRole))
                //{
                //    return RedirectToAction(nameof(Index));
                //}

                // подтверждение почты
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    string userId = await _userManager.GetUserIdAsync(user);
                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    // кодирование токена в строку позволяет передавать его через URL без проблем с символами URL
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme
                    );

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    // HtmlEncoder.Default.Encode(callbackUrl) - для безопасного кодирования URL в HTML

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();  // что-то пошло не так, отображаем форму заново
        }


        // Поддерживает ли используемое хранилище пользователей электронные адреса.
        // Если нет, выбрасывается исключение. Иначе, возвращается экземпляр хранилища электронных
        // адресов, приведенный к типу IUserEmailStore<IdentityUser>
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
