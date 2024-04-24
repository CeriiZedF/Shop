using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using Shop;
using Shop.Services;
using Shop.Areas.API;

using Shop.DAL.Data;
using Shop.DAL.Repository;
using Shop.DAL.Repository.IRepository;


var builder = WebApplication.CreateBuilder(args);

// add base and my services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddMemoryCache();

// add my services
builder.Services.AddSingleton<BasketProductSessionService>();
builder.Services.AddScoped<CurrentUserProvider>();


//////////////////////// SESSION ////////////////////////
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});


//////////////////////// DATABASE ////////////////////////
string connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string is not defined!");

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(connectionString);
});


//////////////////////// IDENTITY ////////////////////////
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
    .AddDefaultTokenProviders()  // добавляет стандартные поставщики токенов для Identity
    .AddDefaultUI()  // добавляет стандартный UI для Identity
    .AddEntityFrameworkStores<DataContext>();  // настраивает Identity для использ. EF в качестве хранилища данных

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.Cookie.Name = "ITShop_Identity";
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Identity/Account/Login";

    // ReturnUrlParameter requires 
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(WC.AdminManagerPolicy, policy => policy.RequireRole(WC.AdminRole, WC.ManagerRole));
    options.AddPolicy(WC.AdminPolicy, policy => policy.RequireRole(WC.AdminRole));
});


//////////////////////// JSON WEB TOKEN ////////////////////////
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWTSettings"));

string secretKey = builder.Configuration.GetSection("JWTSettings:SecretKey").Value;
var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
///////////////////////////////////////////////////////////////


//////////////////////// GOOGLE AUTH and JWT ////////////////////////
builder.Services.AddAuthentication(options =>
{
    //options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    //options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    //options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetSection("JWTSettings:Issure").Value,
            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetSection("JWTSettings:Audience").Value,
            ValidateLifetime = true,
            IssuerSigningKey = signinKey
        };
    });
/////////////////////////////////////////////////////////////


//////////////////////// REPOSITORY ////////////////////////
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductUsageRepository, ProductUsageRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
////////////////////////////////////////////////////////////



var app = builder.Build();

// configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.MapRazorPages();

//////////////////////// FOR IDENTITY ////////////////////////
app.UseAuthentication();
app.UseAuthorization();

//////////////////////// SET ROLES IN DB ////////////////////////
using (var scope = app.Services.CreateScope())
{
    // из области получаем экземпляр сервиса RoleManager<IdentityRole>
    // предназначенная для управления ролями в приложении
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { WC.AdminRole, WC.CustomerRole, WC.ManagerRole };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))  // существует ли роль в системе
        {
            await roleManager.CreateAsync(new IdentityRole(role));  // создаём роль
        }
    }
}
////////////////////////////////////////////////////////////

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}"
);
app.Run();
