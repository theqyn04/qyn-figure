using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qyn_figure.Models;
using qyn_figure.Repository;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly QynFigureContext _context;
        public CouponController (QynFigureContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(_context.Coupons.ToList());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CouponModel coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm mới mã giảm giá thành công";
                return RedirectToAction("Index");
            }

            else
            {
                List<string> errors = new List<string>();
                foreach (var error in ModelState.Values)
                {
                    foreach (var err in error.Errors)
                    {
                        errors.Add(err.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            var coupon = await _context.Coupons.FindAsync(Id);
            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, CouponModel coupon)
        {
            var updateCoupon = await _context.Coupons.FindAsync(Id);

            if (updateCoupon == null) // Add this check
            {
                TempData["error"] = "Không tìm thấy mã giảm giá";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                updateCoupon.Name = coupon.Name;
                updateCoupon.Description = coupon.Description;
                updateCoupon.Status = coupon.Status;
                updateCoupon.Quantity = coupon.Quantity;
                updateCoupon.DateStart = coupon.DateStart;
                updateCoupon.DateEnd = coupon.DateEnd;

                _context.Coupons.Update(updateCoupon);
                await _context.SaveChangesAsync();
                TempData["success"] = "Chỉnh sửa mã giảm giá thành công";
                return RedirectToAction("Index");
            }
            else
            {
                // Return to the Edit view with the model to show validation errors
                return View(coupon);
            }
        }

        public async Task<IActionResult> Remove(int Id)
        {
            CouponModel coupon = await _context.Coupons.FindAsync(Id);
            if (coupon == null)
            {
                TempData["error"] = "Không tìm thấy mã giảm giá";
                return RedirectToAction("Index");
            }

            _context.Coupons.Remove(coupon);
            await _context.SaveChangesAsync(); // Sửa ở đây
            TempData["error"] = "Xóa mã giảm giá thành công";
            return RedirectToAction("Index");
        }
    }
}
