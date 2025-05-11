using qyn_figure.Models;
using qyn_figure.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace qyn_figure.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<AppUserModel> userManager,
            SignInManager<AppUserModel> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Truyền ReturnUrl vào ViewModel
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? Url.Content("~/")
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                // 1. Xử lý ReturnUrl
                model.ReturnUrl = model.ReturnUrl ?? Url.Content("~/");

                // 2. Validate ModelState
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"ModelState invalid. Errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    return View(model);
                }

                // 3. Tìm user bằng username hoặc email
                var user = await _userManager.FindByNameAsync(model.Username) ??
                          await _userManager.FindByEmailAsync(model.Username);

                if (user == null)
                {
                    _logger.LogWarning($"User not found: {model.Username}");
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng");
                    return View(model);
                }

                // 4. Thử đăng nhập
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    model.Password,
                    isPersistent: false,
                    lockoutOnFailure: true); // Bật lockout để phòng brute force

                // 5. Xử lý kết quả
                if (result.Succeeded)
                {
                    _logger.LogInformation($"User {user.UserName} logged in successfully");

                    // Kiểm tra nếu ReturnUrl là local URL để phòng open redirect attack
                    if (Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.UserName} locked out");
                    ModelState.AddModelError(string.Empty, "Tài khoản tạm thời bị khóa do đăng nhập sai quá nhiều lần");
                    return View(model);
                }
                else
                {
                    _logger.LogWarning($"Invalid password for user {user.UserName}");
                    ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu không đúng");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đăng nhập");
                return View(model);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CheckAdminAccount()
        {
            var user = await _userManager.FindByNameAsync("admin@example.com");
            if (user == null)
            {
                return Content("Admin account not found");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, "Admin@123");
            var roles = await _userManager.GetRolesAsync(user);

            return Content($"User exists: {user.UserName}\n" +
                         $"Password valid: {passwordValid}\n" +
                         $"Roles: {string.Join(", ", roles)}");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(UserModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUserModel
                {
                    UserName = model.Username,
                    Email = model.Email,
                    EmailConfirmed = true // Bỏ qua xác nhận email nếu không cần
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán role "User" mặc định
                    var roleResult = await _userManager.AddToRoleAsync(user, "Customer");

                    if (!roleResult.Succeeded)
                    {
                        // Ghi log nếu gán role thất bại
                        _logger.LogWarning($"Không thể gán role User cho user {user.UserName}");

                        // Vẫn cho phép đăng nhập dù gán role thất bại
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["success"] = "Đăng ký thành công!";
                    return RedirectToAction("Login", "Account");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            TempData["error"] = "Đăng ký thất bại!";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Đăng xuất thành công");
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResetAdminAccount()
        {
            // Xóa admin cũ nếu tồn tại
            var oldAdmin = await _userManager.FindByNameAsync("admin@example.com");
            if (oldAdmin != null)
            {
                await _userManager.DeleteAsync(oldAdmin);
            }

            // Tạo user mới
            var admin = new AppUserModel
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true
            };

            // Tạo password mới
            var result = await _userManager.CreateAsync(admin, "Admin@123");

            if (result.Succeeded)
            {
                // Tạo role Admin nếu chưa có
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Gán role
                await _userManager.AddToRoleAsync(admin, "Admin");

                return Content("Đã tạo lại admin thành công!\nUsername: admin@example.com\nPassword: Admin@123");
            }

            return Content($"Lỗi khi tạo admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
}