using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly QynFigureContext _context;

        public BrandController(QynFigureContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(_context.Brands.ToList());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm mới loại sản phẩm thành công";
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
        public async Task<IActionResult> Edit(int BrandId)
        {
            var brand = await _context.Brands.FindAsync(BrandId);
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int BrandId, BrandModel brand)
        {
            var updateBrand = await _context.Brands.FindAsync(brand);

            if (updateBrand == null) // Add this check
            {
                TempData["error"] = "Không tìm thấy thương hiệu";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                updateBrand.Name = updateBrand.Name;
                updateBrand.Description = updateBrand.Description;

                _context.Brands.Update(updateBrand);
                await _context.SaveChangesAsync();
                TempData["success"] = "Chỉnh sửa thương hiệu thành công";
                return RedirectToAction("Index");
            }
            else
            {
                // Return to the Edit view with the model to show validation errors
                return View(brand);
            }
        }

        public async Task<IActionResult> Remove(int BrandId)
        {
            BrandModel brand = await _context.Brands.FindAsync(BrandId);
            _context.Brands.Remove(brand);
            _context.SaveChanges();
            TempData["error"] = "Xóa thương hiệu thành công";
            return RedirectToAction("Index");
        }
    }
}
