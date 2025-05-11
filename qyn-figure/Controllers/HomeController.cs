using System.Diagnostics;
using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace qyn_figure.Controllers
{
    public class HomeController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ILogger<HomeController> logger,
            QynFigureContext context,
            UserManager<AppUserModel> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            var sliders = _context.Sliders.ToList();
            ViewBag.Sliders = sliders;
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        public async Task<IActionResult> CreateAdminAccount()
        {
            // Kiểm tra xem admin đã tồn tại chưa
            var adminExists = await _userManager.FindByEmailAsync("admin@example.com");
            if (adminExists != null)
            {
                return Content("Admin account already exists");
            }

            var adminUser = new AppUserModel
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                // Kiểm tra và tạo role Admin nếu chưa có
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Gán role Admin cho user
                await _userManager.AddToRoleAsync(adminUser, "Admin");

                return Content("Admin account created successfully. Username: admin@example.com, Password: Admin@123");
            }

            return Content($"Error creating admin account: {string.Join(", ", result.Errors)}");
        }
    }
}