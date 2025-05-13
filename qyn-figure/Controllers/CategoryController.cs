using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Controllers
{
    public class CategoryController : Controller
    {
        private readonly QynFigureContext _context;

        public CategoryController(QynFigureContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string Name = "", string sort_by = "", int? startprice = null, int? endprice = null)
        {
            // Xử lý khi không có tên danh mục
            if (string.IsNullOrEmpty(Name))
            {
                var allProducts = await _context.Products.ToListAsync();
                return View(allProducts);
            }

            // Tìm danh mục
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == Name);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                return RedirectToAction("Index", "Home");
            }

            // Lấy sản phẩm
            var query = _context.Products
                .Where(p => p.CategoryId == category.Id);

            // Lọc theo khoảng giá nếu có
            if (startprice.HasValue && endprice.HasValue)
            {
                query = query.Where(p => p.Price >= startprice.Value && p.Price <= endprice.Value);
            }
            else if (startprice.HasValue)
            {
                query = query.Where(p => p.Price >= startprice.Value);
            }
            else if (endprice.HasValue)
            {
                query = query.Where(p => p.Price <= endprice.Value);
            }

            // Xử lý sắp xếp
            query = sort_by switch
            {
                "price_increase" => query.OrderBy(p => p.Price),
                "price_decrease" => query.OrderByDescending(p => p.Price),
                "price_newest" => query.OrderByDescending(p => p.Id),
                "price_oldest" => query.OrderBy(p => p.Id),
                _ => query.OrderByDescending(p => p.Id) // Mặc định
            };

            return View(await query.ToListAsync());
        }
    }
}
