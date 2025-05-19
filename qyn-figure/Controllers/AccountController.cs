using qyn_figure.Models;
using qyn_figure.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using qyn_figure.Repository;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Areas.Admin.Repository;
using System.Net;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

namespace qyn_figure.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly SignInManager<AppUserModel> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;
        private readonly QynFigureContext _context;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<AppUserModel> userManager,
            SignInManager<AppUserModel> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AccountController> logger,
            QynFigureContext context,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _context = context;
            _emailSender = emailSender;
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

        //Đăng kí tài khoản
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


        //Logout method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
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

        //Method xem lại lịch sử đơn hàng
        public async Task<IActionResult> History()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var Orders = await _context.Orders.Where(od => od.CustomerId == userId).ToListAsync();
            return View(Orders);
        }

        //Method hủy đơn hàng bởi user
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            try
            {
                var order = await _context.Orders.Where(o => o.OrderId == orderId).FirstAsync();
                order.Status = "Canceled";
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("An error while canceling the order.");
            }
            return RedirectToAction("History", "Account");
        }

        


        //Method to update user account infomation
        public async Task<IActionResult> UpdateAccount()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) 
            { 
                return NotFound();
            }
            return View(user);
        }

        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userById = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            else
            {
                _context.Update(userById);
                await _context.SaveChangesAsync();
                TempData["success"] = "Cập nhật thông tin thành công.";
            }
            return RedirectToAction("UpdateAccount", "Account");
        }


        //Method login by Google OAth
        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            try
            {
                // Xóa cookie hiện có trước khi thử đăng nhập
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    // Thử cách khác để lấy thông tin
                    var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                    if (result?.Principal != null)
                    {
                        info = new ExternalLoginInfo(
                            result.Principal,
                            result.Properties.Items[".AuthScheme"],
                            result.Principal.FindFirstValue(ClaimTypes.NameIdentifier),
                            result.Principal.FindFirstValue(ClaimTypes.Name))
                        {
                            AuthenticationTokens = result.Properties.GetTokens()
                        };
                    }

                    if (info == null)
                    {
                        TempData["error"] = "Lỗi: Không thể lấy thông tin từ Google. Vui lòng thử lại.";
                        return RedirectToAction("Login");
                    }
                }

                // Lấy email từ Google
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email))
                {
                    TempData["error"] = "Không thể lấy email từ Google.";
                    return RedirectToAction("Login");
                }

                // Kiểm tra user đã tồn tại chưa
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Tạo user mới với mật khẩu mặc định
                    user = new AppUserModel
                    {
                        UserName = email.Split('@')[0],
                        Email = email,
                        EmailConfirmed = true  // Xác thực email luôn vì dùng Google
                    };

                    // Tạo user với mật khẩu cứng "User123@"
                    var createResult = await _userManager.CreateAsync(user, "User123@");

                    if (!createResult.Succeeded)
                    {
                        TempData["error"] = "Không thể tạo tài khoản.";
                        return RedirectToAction("Login");
                    }

                    // Thêm role mặc định (nếu cần)
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                // Đăng nhập bằng Cookie (không cần Google lần sau)
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex}");
                TempData["error"] = "Đã xảy ra lỗi hệ thống.";
                return RedirectToAction("Login");
            }
        }
    }


}