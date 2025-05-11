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



        public async Task<IActionResult> Index(string Name = "")
        {
            // Nếu không có tên danh mục, hiển thị tất cả sản phẩm hoặc thông báo
            if (string.IsNullOrEmpty(Name))
            {
                // Có thể chuyển hướng hoặc hiển thị tất cả sản phẩm
                var allProducts = await _context.Products.ToListAsync();
                return View(allProducts);
            }

            // Tìm danh mục theo tên
            CategoryModel category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == Name);

            if (category == null)
            {
                // Nếu không tìm thấy danh mục, có thể hiển thị thông báo
                TempData["ErrorMessage"] = "Không tìm thấy danh mục!";
                return RedirectToAction("Index", "Home"); // Hoặc trả về view với Model rỗng
            }

            // Lấy danh sách sản phẩm thuộc danh mục
            var productByCategory = await _context.Products
                .Where(p => p.CategoryId == category.Id)
                .ToListAsync();

            return View(productByCategory);
        }


    }
}
