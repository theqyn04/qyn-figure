using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly QynFigureContext _context;

        public CategoryController(QynFigureContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(_context.Categories.ToList());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CategoryModel category)
        {
            if (ModelState.IsValid)
            {              
                _context.Categories.Add(category);
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
        public async Task<IActionResult> Edit(int CategoryId)
        {
            var category = await _context.Categories.FindAsync(CategoryId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int CategoryId, CategoryModel category)
        {
            var updateCategory = await _context.Categories.FindAsync(CategoryId);

            if (updateCategory == null) // Add this check
            {
                TempData["error"] = "Không tìm thấy loại sản phẩm";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                updateCategory.Name = category.Name;
                updateCategory.Description = category.Description;

                _context.Categories.Update(updateCategory);
                await _context.SaveChangesAsync();
                TempData["success"] = "Chỉnh sửa loại sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                // Return to the Edit view with the model to show validation errors
                return View(category);
            }
        }

        public async Task<IActionResult> Remove(int CategoryId)
        {
            CategoryModel category = await _context.Categories.FindAsync(CategoryId);
            _context.Categories.Remove(category);
            _context.SaveChanges();
            TempData["error"] = "Xóa loại sản phẩm thành công";
            return RedirectToAction("Index");
        }
    }
}
