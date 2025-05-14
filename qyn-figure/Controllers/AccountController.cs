using qyn_figure.Models;
using qyn_figure.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using qyn_figure.Repository;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Areas.Admin.Repository;

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

        //Gửi mail để reset password
        public async Task<IActionResult> SendMailForgetPass(AppUserModel user)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

            if (checkMail == null)
            {
                TempData["error"] = "Không tìm thấy email này";
                return RedirectToAction("ForgetPass", "Account");
            }
            else
            {
                var receiver = checkMail.Email;
                var subject = "Yêu cầu thay đổi mật khẩu cho tài khoản " + checkMail.Email;
                var message = "Click vào đây để đổi mật khẩu " +
                    "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass?email=" + checkMail.Email + "'>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "Chúng tôi đã gửi một tin nhắn đến email bạn đã đăng kí, hãy check mail để thay đổi mật khẩu.";
            return RedirectToAction("ForgetPass", "Account");
        }


        public async Task<IActionResult> ForgetPass(string returnUrl)
        {
            return View();
        }

        public async Task<IActionResult> NewPass(AppUserModel user)
        {
            var checkUser = await _userManager.Users.Where(u => u.Email == user.Email).FirstOrDefaultAsync();

            if (checkUser!= null)
            { 
                ViewBag.Email = checkUser.Email;
            }
            else
            {
                TempData["error"] = "Email không tìm thấy";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }

        //Cập nhật mật khẩu mới
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user)
        {
            var checkUser = await _userManager.Users.Where(u => u.Email == user.Email).FirstOrDefaultAsync();

            if (checkUser != null)
            {
               var passwordHasher = new PasswordHasher<AppUserModel>();
               var passwordHash = passwordHasher.HashPassword(checkUser, user.PasswordHash);

               checkUser.PasswordHash = passwordHash;
               await _userManager.UpdateAsync(checkUser);
                TempData["success"] = "Đã thay đổi password thành công.";
                return RedirectToAction("Login", "Account");
            }
            else
            {
                TempData["error"] = "Email không tìm thấy";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }
    }


}