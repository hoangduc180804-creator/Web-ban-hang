using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option =>
                {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });

// 2. C?u hình Cookie Authentication cho Khách hàng (Shop)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option =>
                {
                    option.Cookie.Name = "SV22T1020811.Shop";
                    option.LoginPath = "/Account/Login";
                    option.AccessDeniedPath = "/Account/AccessDenied";
                    option.ExpireTimeSpan = TimeSpan.FromDays(7);
                    option.SlidingExpiration = true;
                    option.Cookie.HttpOnly = true;
                });

// 3. C?u hình Session
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

var app = builder.Build();

// 4. Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// C?U HÌNH FILE T?NH (STATIC FILES)
app.UseStaticFiles(); // Cho wwwroot c?a Shop

// --- ?O?N NÀY GIÚP HI?N TH? ?NH T? ADMIN ---
// T? ??ng tìm th? m?c Admin/wwwroot/images/products d?a trên v? trí project Shop
var adminImagesPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "SV22T1020811.Admin", "wwwroot", "images", "products"));

if (Directory.Exists(adminImagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(adminImagesPath),
        RequestPath = "/images/products"
    });
}
// ------------------------------------------

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// 5. C?u hình ??nh d?ng ti?ng Vi?t
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// 6. Kh?i t?o Business Layer
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
    ?? throw new InvalidOperationException("ConnectionString 'LiteCommerceDB' not found.");

SV22T1020811.BusinessLayers.Configuration.Initialize(connectionString);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();