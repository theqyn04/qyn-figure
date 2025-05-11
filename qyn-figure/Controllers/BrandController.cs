using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Models;
using qyn_figure.Repository;

namespace qyn_figure.Controllers
{
    public class BrandController : Controller
    {
        private readonly QynFigureContext _context;

        public BrandController(QynFigureContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(string Name = "")
        {
            // Nếu không có tên thương hiệu, hiển thị tất cả sản phẩm hoặc thông báo
            if (string.IsNullOrEmpty(Name))
            {
                // Có thể chuyển hướng hoặc hiển thị tất cả sản phẩm
                var allProducts = await _context.Products.ToListAsync();
                return View(allProducts);
            }

            // Tìm thương hiệu theo tên
            BrandModel brand = await _context.Brands
                .FirstOrDefaultAsync(c => c.Name == Name);

            if (brand == null)
            {
                // Nếu không tìm thấy thương hiệu, có thể hiển thị thông báo
                TempData["ErrorMessage"] = "Không tìm thấy thương hiệu!";
                return RedirectToAction("Index", "Home"); // Hoặc trả về view với Model rỗng
            }

            // Lấy danh sách sản phẩm thuộc danh mục
            var productByBrand = await _context.Products
                .Where(p => p.BrandId == brand.Id)
                .ToListAsync();

            return View(productByBrand);
        }

    }
}
