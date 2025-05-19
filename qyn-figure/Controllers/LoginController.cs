using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using qyn_figure.Models.ViewModels;
using qyn_figure.Models;
using qyn_figure.Services;
using qyn_figure.Areas.Admin.Repository;

namespace qyn_figure.Controllers
{
    public class LoginController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IOtpService _otpService;

        public LoginController(
            UserManager<AppUserModel> userManager,
            IEmailSender emailSender,
            IOtpService otpService)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _otpService = otpService;
        }

        // GET: Giao diện nhập email để gửi OTP
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Xử lý yêu cầu gửi OTP
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Không tiết lộ user không tồn tại
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Tạo và gửi OTP
            var otp = _otpService.GenerateOtp(model.Email);

            // Gửi email chứa OTP
            await _emailSender.SendEmailAsync(
                model.Email,
                "Mã OTP đặt lại mật khẩu",
                $"Mã OTP của bạn là: {otp}. Mã có hiệu lực trong 5 phút.");

            return RedirectToAction("VerifyOtp", new { email = model.Email });
        }

        // GET: Giao diện nhập OTP
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            var model = new VerifyOtpViewModel { Email = email };
            return View(model);
        }

        // POST: Xác thực OTP
        [HttpPost]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_otpService.ValidateOtp(model.Email, model.Otp))
            {
                ModelState.AddModelError("", "Mã OTP không hợp lệ hoặc đã hết hạn");
                return View(model);
            }

            // Lưu trạng thái xác thực vào session
            HttpContext.Session.SetString("OTP_Verified", model.Email);

            return RedirectToAction("ResetPassword", new { email = model.Email });
        }

        // GET: Giao diện đặt lại mật khẩu
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            // Kiểm tra xác thực OTP
            if (HttpContext.Session.GetString("OTP_Verified") != email)
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel { Email = email };
            return View(model);
        }

        // POST: Xử lý đặt lại mật khẩu
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Kiểm tra xác thực OTP
            if (HttpContext.Session.GetString("OTP_Verified") != model.Email)
            {
                return RedirectToAction("ForgotPassword");
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            // Đặt lại mật khẩu
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.Password);

            if (result.Succeeded)
            {
                // Xóa session sau khi đổi mật khẩu thành công
                HttpContext.Session.Remove("OTP_Verified");
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        // Các action confirmation
        public IActionResult ForgotPasswordConfirmation() => View();
        public IActionResult ResetPasswordConfirmation() => View();

    }
}
