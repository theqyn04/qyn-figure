using qyn_figure.Models;
using qyn_figure.Models.ViewModels;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Controllers
{
    public class ProductController : Controller
    {
        private readonly QynFigureContext _context;
        public ProductController(QynFigureContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int Id)
        {
            if (Id <= 0) return RedirectToAction("Index");

            var productDetails = await _context.Products
                .Include(p => p.ProductImages) // Thêm dòng này để load cả ảnh sản phẩm
                .FirstOrDefaultAsync(c => c.Id == Id);

            if (productDetails == null) return RedirectToAction("Index");

            // Lấy ảnh mặc định (nếu có)
            var defaultImage = productDetails.ProductImages.FirstOrDefault(i => i.IsDefault == true) ??
                              productDetails.ProductImages.FirstOrDefault();

            // Gán ảnh mặc định vào ImageUrl nếu chưa có
            if (defaultImage != null && string.IsNullOrEmpty(productDetails.ImageUrl))
            {
                productDetails.ImageUrl = defaultImage.ImageUrl;
            }

            // Sản phẩm liên quan
            var relatedProducts = await _context.Products
                .Where(c => c.CategoryId == productDetails.CategoryId && c.Id != Id)
                .OrderByDescending(c => c.Id)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            var viewModel = new ProductDetailsViewModel
            {
                Product = productDetails,
                ProductImages = productDetails.ProductImages.OrderBy(i => i.DisplayOrder).ToList()
            };

            return View(viewModel);
        }

        //Method search
        public async Task<IActionResult> Search(string searchTerm)
        {
            //Tìm kiếm sản phẩm có tên hoặc mô tả trùng với keyword
            var products = await _context.Products.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)).ToListAsync();
            ViewBag.KeyWord = searchTerm;

            return View(products);

        }

        //Sử lý rating từ user
        [HttpPost]
        public async Task<IActionResult> RatingProduct(RatingModel rating)
        {
            //Nếu k lỗi thì thêm rating vào db
            if (ModelState.IsValid)
            {
                var ratingEntity = new RatingModel
                {
                    Id = rating.Id,
                    Name = rating.Name,
                    Comment = rating.Comment,
                    Star = rating.Star,
                    CreatedAt = DateTime.Now,
                    Email = rating.Email
                };

                _context.Ratings.Add(ratingEntity);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cảm ơn bạn đã đánh giá sản phẩm này!";

                //Chuyển hướng về trang trước đó
                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra trong quá trình đánh giá sản phẩm!";
                List<string> errors = new List<string>();
                foreach (var error in ModelState.Values)
                {
                    foreach (var err in error.Errors)
                    {
                        errors.Add(err.ErrorMessage);
                    }
                }
                string errorMessage = string.Join(", ", errors);
                //Quay lại trang detail với id của sản phẩm trước đó
                return RedirectToAction("Details", "Product", new { Id = rating.Id });
            }
            return Redirect(Request.Headers["Referer"]);
        }
    }
}
