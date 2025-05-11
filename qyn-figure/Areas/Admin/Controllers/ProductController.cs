using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly IWebHostEnvironment _webHostEn;
        public ProductController(QynFigureContext context, IWebHostEnvironment webHostEn)
        {
            _context = context;
            _webHostEn = webHostEn;
        }

        //Phân trang cho Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<ProductModel> product = _context.Products.ToList();


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = product.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            var data = product.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }

        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Brand = new SelectList(_context.Brands.ToList(), "Id", "Name");
            ViewBag.Series = new SelectList(_context.Series.ToList(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductModel product)
        {
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Brand = new SelectList(_context.Brands.ToList(), "Id", "Name");
            ViewBag.Series = new SelectList(_context.Series.ToList(), "Id", "Name");

            if (ModelState.IsValid)
            {
                if (product.ImageUpload != null)
                {
                    string upLoadDir = Path.Combine(_webHostEn.WebRootPath, "img/product_img");
                    string imgName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
                    string filePath = Path.Combine(upLoadDir, imgName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    product.ImageUrl = "img/product_img/" + imgName;
                }
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm sản phẩm";
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Brand = new SelectList(_context.Brands.ToList(), "Id", "Name");
            ViewBag.Series = new SelectList(_context.Series.ToList(), "Id", "Name");
            var product = await _context.Products.FindAsync(Id);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ProductModel product)
        {
            ViewBag.Category = new SelectList(_context.Categories.ToList(), "Id", "Name");
            ViewBag.Brand = new SelectList(_context.Brands.ToList(), "Id", "Name");
            ViewBag.Series = new SelectList(_context.Series.ToList(), "Id", "Name");

            if (Id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updateProduct = await _context.Products.FindAsync(Id);

                    if (product.ImageUpload != null)
                    {
                        string upLoadDir = Path.Combine(_webHostEn.WebRootPath, "img/product_img");
                        string imgName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(product.ImageUpload.FileName);
                        string filePath = Path.Combine(upLoadDir, imgName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(fs);
                        }
                        updateProduct.ImageUrl = "img/product_img/" + imgName;
                    }

                    updateProduct.Name = product.Name;
                    updateProduct.CategoryId = product.CategoryId;
                    updateProduct.BrandId = product.BrandId;
                    updateProduct.SeriesId = product.SeriesId;
                    updateProduct.Price = product.Price;
                    updateProduct.StockQuantity = product.StockQuantity;
                    updateProduct.Description = product.Description;

                    _context.Products.Update(updateProduct);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật sản phẩm thành công";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(product);
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Remove(int Id)
        {
            ProductModel product = await _context.Products.FindAsync(Id);
            _context.Products.Remove(product);
            _context.SaveChanges();
            TempData["error"] = "Xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
    }
}
