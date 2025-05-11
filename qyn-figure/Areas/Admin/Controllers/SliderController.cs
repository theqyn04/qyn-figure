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
    public class SliderController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly IWebHostEnvironment _webHostEn;
        public SliderController(QynFigureContext context, IWebHostEnvironment webHostEn)
        {
            _context = context;
            _webHostEn = webHostEn;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Sliders.ToListAsync());
        }

        [HttpGet]
        public IActionResult Add()
        {           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(SliderModel slider)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý upload ảnh
                    if (slider.ImageFile != null && slider.ImageFile.Length > 0)
                    {
                        string uploadsDir = Path.Combine(_webHostEn.WebRootPath, "img/slider_img");

                        if (!Directory.Exists(uploadsDir))
                        {
                            Directory.CreateDirectory(uploadsDir);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + slider.ImageFile.FileName;
                        string filePath = Path.Combine(uploadsDir, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await slider.ImageFile.CopyToAsync(fileStream);
                        }

                        slider.Image = "img/slider_img/" + uniqueFileName;
                    }

                    // Thiết lập giá trị CreatedAt
                    slider.CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    _context.Sliders.Add(slider);
                    await _context.SaveChangesAsync();

                    TempData["success"] = "Thêm slider thành công";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Lỗi khi thêm slider: " + ex.Message;
                    return View(slider);
                }
            }

            // Hiển thị lỗi validation
            var errorMessages = ModelState.Values
                                .SelectMany(v => v.Errors)
                                .Select(e => e.ErrorMessage)
                                .ToList();

            TempData["error"] = "Lỗi dữ liệu:<br>" + string.Join("<br>", errorMessages);
            return View(slider);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int Id)
        {         
            var slider = await _context.Sliders.FindAsync(Id);
            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, SliderModel slider)
        {
            var updateSlider = await _context.Sliders.FindAsync(Id);          
            if (ModelState.IsValid)
            {
                if (slider.ImageFile != null)
                {
                    string upLoadDir = Path.Combine(_webHostEn.WebRootPath, "img/slider_img");
                    string imgName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(slider.ImageFile.FileName);
                    string filePath = Path.Combine(upLoadDir, imgName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await slider.ImageFile.CopyToAsync(fs);
                    }
                    updateSlider.Image = "img/slider_img/" + imgName;
                }
                updateSlider.Name = slider.Name;
                updateSlider.Description = slider.Description;
                updateSlider.Status = slider.Status;

                _context.Sliders.Update(updateSlider);
                await _context.SaveChangesAsync();
                TempData["success"] = "Chỉnh sửa slider thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Vui lòng chọn file thích hợp!";
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

        public async Task<IActionResult> Remove(int Id)
        {
            SliderModel slider = await _context.Sliders.FindAsync(Id);
            _context.Sliders.Remove(slider);
            _context.SaveChanges();
            TempData["error"] = "Xóa sldier thành công";
            return RedirectToAction("Index");
        }
    }
}
