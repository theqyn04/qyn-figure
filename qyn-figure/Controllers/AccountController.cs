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

        [HttpGet]
        public IActionResult ForgetPass()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgetPass(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Vui lòng nhập email";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Không tiết lộ user không tồn tại (bảo mật)
                TempData["Success"] = "Nếu email tồn tại, liên kết đặt lại mật khẩu đã được gửi";
                return RedirectToAction(nameof(ForgetPass));
            }

            try
            {
                // Tạo token và mã hóa đúng cách
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Tạo link reset
                var resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { email = user.Email, token = encodedToken }, // Dùng token đã mã hóa
                    Request.Scheme);

                // Gửi email
                var subject = "Đặt lại mật khẩu - QYN Figure";
                var message = $@"
            <h3>Yêu cầu đặt lại mật khẩu</h3>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản {user.Email}.</p>
            <p>Vui lòng nhấp vào liên kết sau để đặt lại mật khẩu:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
            <p>Liên kết này sẽ hết hạn sau 24 giờ.</p>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>";

                await _emailSender.SendEmailAsync(user.Email, subject, message);

                TempData["Success"] = "Đã gửi liên kết đặt lại mật khẩu đến email của bạn. Vui lòng kiểm tra hộp thư.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email đặt lại mật khẩu");
                TempData["Error"] = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
            }

            return RedirectToAction(nameof(ForgetPass));
        }

        public async Task<IActionResult> NewPass(string email) // Thay đổi tham số từ AppUserModel sang string
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["error"] = "Email không được để trống";
                return RedirectToAction("ForgetPass");
            }

            var checkUser = await _userManager.FindByEmailAsync(email); // Sử dụng FindByEmailAsync thay vì query trực tiếp
            if (checkUser == null)
            {
                TempData["error"] = "Email không tìm thấy";
                return RedirectToAction("ForgetPass");
            }

            ViewBag.Email = checkUser.Email;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Liên kết không hợp lệ";
                return RedirectToAction("ForgetPass");
            }

            try
            {
                // Giải mã token
                var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

                var model = new ResetPasswordViewModel
                {
                    Email = email,
                    Token = decodedToken
                };

                return View(model);
            }
            catch
            {
                TempData["Error"] = "Token không hợp lệ";
                return RedirectToAction("ForgetPass");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Không tiết lộ user không tồn tại
                TempData["Success"] = "Mật khẩu đã được đặt lại thành công";
                return RedirectToAction("Login");
            }

            // Log token để debug
            _logger.LogInformation($"Using token: {model.Token}");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                // Cập nhật SecurityStamp để vô hiệu hóa token cũ
                await _userManager.UpdateSecurityStampAsync(user);

                TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Login");
            }

            // Xử lý lỗi chi tiết
            foreach (var error in result.Errors)
            {
                _logger.LogError($"Reset password error: {error.Code} - {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
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


        //Method login by Google
        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            });
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result =  await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            {
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });
            TempData["success"] = "Đăng nhập thành công.";
            return RedirectToAction("Index", "Home");
        }
    }


}