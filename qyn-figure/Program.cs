using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using qyn_figure.Areas.Admin.Repository;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace qyn_figure
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<QynFigureContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Add email sender
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddScoped<QynFigureContext>();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            // Cấu hình Cookie Policy
            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
            });

            // Cấu hình Cookie Authentication
            // Sửa lại phần cấu hình Cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.SameSite = SameSiteMode.None; // Thay đổi từ Lax sang None
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.HttpOnly = true;
            });

            builder.Services.AddIdentity<AppUserModel, IdentityRole>()
    .AddEntityFrameworkStores<QynFigureContext>().AddDefaultTokenProviders();


            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.AllowedForNewUsers = false; // Tắt lockout cho user mới
                options.Lockout.MaxFailedAccessAttempts = 10; // Số lần thử tối đa (đặt lớn để vô hiệu hóa)
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1); // Thời gian lock ngắn

                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 0;

                // User settings.
                options.User.RequireUniqueEmail = false;
            });

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            builder.Services.AddControllersWithViews(options =>
            {
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
                    _ => "Trường này là bắt buộc");
            });

            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\temp-keys\"))
                .SetApplicationName("qyn_figure").SetDefaultKeyLifetime(TimeSpan.FromDays(14)); // Tăng thời hạn key

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(2); // Giảm thời gian để test
            });



            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Google:ClientId"];
    options.ClientSecret = builder.Configuration["Google:ClientSecret"];

    // Thêm các cấu hình quan trọng sau
    options.SaveTokens = true;
    options.CorrelationCookie.SameSite = SameSiteMode.None;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events = new OAuthEvents()
    {
        OnCreatingTicket = context =>
        {
            // Log thông tin ticket để debug
            Console.WriteLine($"Access Token: {context.AccessToken}");
            return Task.CompletedTask;
        },
        OnRemoteFailure = context =>
        {
            Console.WriteLine($"Remote failure: {context.Failure.Message}");
            context.Response.Redirect("/Account/Login");
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
});


            var app = builder.Build();

            app.UseCookiePolicy();

            app.UseStatusCodePagesWithRedirects("/Home/Error?statusCode={0}");

            app.UseSession();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Request Path: {context.Request.Path}");
                Console.WriteLine($"Cookies: {string.Join(", ", context.Request.Cookies.Keys)}");
                await next();
            });

            app.Run();
        }
    }
}
