using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using qyn_figure.Models.OrderInfo;
using qyn_figure.Services;

namespace qyn_figure.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;

        public PaymentController(IMomoService momoService)
        {
            _momoService = momoService;
        }

        
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            try
            {
                var response = await _momoService.CreatePaymentMomo(model);

                if (response == null || string.IsNullOrEmpty(response.PayUrl))
                {
                    // Log lỗi và hiển thị thông báo cho người dùng
                    Console.WriteLine($"Momo response error: {JsonSerializer.Serialize(response)}");
                    TempData["ErrorMessage"] = "Không thể kết nối với cổng thanh toán Momo. Vui lòng thử lại sau.";
                    return RedirectToAction("Index", "Cart");
                }

                return Redirect(response.PayUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Momo payment error: {ex}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán";
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;
            if (requestQuery["resultCode"] == 0)
            {
                TempData["success"] = "Đã hủy giao dịch Momo";
                return RedirectToAction("Index", "Home");
            }
            return View(response);
        }
    }
}
