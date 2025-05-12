using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Models;
using qyn_figure.Models.ViewModels;
using qyn_figure.Repository;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductImageController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly IWebHostEnvironment _webHostEn;
        public ProductImageController(QynFigureContext context, IWebHostEnvironment webHostEn)
        {
            _context = context;
            _webHostEn = webHostEn;
        }

        //Phân trang cho Index
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<ProductImageModel> product = _context.ProductImages.ToList();


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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductImageModel product)
        {
           
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    string upLoadDir = Path.Combine(_webHostEn.WebRootPath, "img/product_img");
                    string imgName = Guid.NewGuid().ToString() + "_" + product.ImageFile.FileName;
                    string filePath = Path.Combine(upLoadDir, imgName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await product.ImageFile.CopyToAsync(fs);
                    fs.Close();
                    product.ImageUrl = "img/product_img/" + imgName;
                }
                _context.ProductImages.Add(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Thêm ảnh sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm ảnh sản phẩm";
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {            
            var image = await _context.ProductImages.FindAsync(Id);
            return View(image);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, ProductImageModel product)
        {
           

            if (Id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var image = await _context.ProductImages.FindAsync(Id);

                    if (product.ImageFile != null)
                    {
                        string upLoadDir = Path.Combine(_webHostEn.WebRootPath, "img/product_img");
                        string imgName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(product.ImageFile.FileName);
                        string filePath = Path.Combine(upLoadDir, imgName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(fs);
                        }
                        image.ImageUrl = "img/product_img/" + imgName;
                    }

                    image.ProductId = product.ProductId;
                    image.DisplayOrder = 1;
                    image.IsDefault = false;
                    image.CreatedDate = DateTime.Now;

                    _context.ProductImages.Update(image);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Cập nhật ảnh sản phẩm thành công";
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImageExists(product.Id))
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

        private bool ImageExists(int id)
        {
            return _context.ProductImages.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Remove(int Id)
        {
            ProductImageModel product = await _context.ProductImages.FindAsync(Id);
            _context.ProductImages.Remove(product);
            _context.SaveChanges();
            TempData["error"] = "Xóa ảnh sản phẩm thành công";
            return RedirectToAction("Index");
        }
    }
}
