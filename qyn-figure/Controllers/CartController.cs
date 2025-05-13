using qyn_figure.Repository;
using Microsoft.AspNetCore.Mvc;
using qyn_figure.Models;
using qyn_figure.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Controllers
{
    public class CartController : Controller
    {
        private QynFigureContext _context;
        public CartController(QynFigureContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<CartModel> cartItem = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();

            var cartVM = new CartViewModel()
            {
                CartItems = cartItem,
                TotalPrice = cartItem.Sum(x => x.Quantity * x.Price),
                CouponCode = HttpContext.Session.GetString("AppliedCoupon")
            };

            // Tính toán giảm giá nếu có coupon
            if (!string.IsNullOrEmpty(cartVM.CouponCode))
            {
                var discount = HttpContext.Session.GetInt32("CouponDiscount") ?? 0;
                cartVM.Discount = (cartVM.TotalPrice * discount) / 100; // Giả sử discount là %
                                                                        // Hoặc cartVM.Discount = discount; // Nếu discount là số tiền cố định
            }

            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        public async Task<IActionResult> AddToCart(int Id)
        {
            ProductModel product = await _context.Products.FindAsync(Id);
            if (product != null)
            {
                List<CartModel> cartItem = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();
                CartModel item = cartItem.FirstOrDefault(x => x.ProductId == Id);
                if (item == null)
                {
                    cartItem.Add(new CartModel(product));
                }
                else
                {
                    item.Quantity++;
                }
                HttpContext.Session.SetJson("cart", cartItem);
            }

            TempData["Success"] = "Thêm vào giỏ hàng thành công!";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Increase(int Id)
        {
            List<CartModel> cart = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();
            CartModel item = cart.FirstOrDefault(x => x.ProductId == Id);

            if (item != null)
            {
                item.Quantity++; // Luôn tăng số lượng bất kể giá trị hiện tại
                HttpContext.Session.SetJson("cart", cart);
            }

            return RedirectToAction("Index");
        }

        // Thêm action giảm số lượng
        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartModel> cart = HttpContext.Session.GetJson<List<CartModel>>("cart");
            CartModel item = cart.FirstOrDefault(x => x.ProductId == Id);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }

                if (cart.Count > 0)
                {
                    HttpContext.Session.SetJson("cart", cart);
                }
                else
                {
                    HttpContext.Session.Remove("cart");
                }
            }

            return RedirectToAction("Index");
        }

        //Method GetCoupon
        [HttpPost]
        public async Task<IActionResult> GetCoupon(string coupon_value)
        {
            try
            {
                var validCoupon = await _context.Coupons
                    .FirstOrDefaultAsync(x => x.Name == coupon_value && x.Status == 1);

                if (validCoupon == null)
                {
                    return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết hiệu lực" });
                }

                if (validCoupon.DateStart > DateTime.Now)
                {
                    return Json(new { success = false, message = "Mã giảm giá chưa có hiệu lực" });
                }

                if (validCoupon.DateEnd < DateTime.Now)
                {
                    return Json(new { success = false, message = "Mã giảm giá đã hết hạn" });
                }

                if (validCoupon.Quantity <= 0)
                {
                    return Json(new { success = false, message = "Mã giảm giá đã hết số lượng" });
                }

                // Lưu thông tin coupon vào session
                HttpContext.Session.SetString("AppliedCoupon", validCoupon.Name);
                

                return Json(new
                {
                    success = true,
                    message = $"Áp dụng mã {validCoupon.Name} thành công"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
