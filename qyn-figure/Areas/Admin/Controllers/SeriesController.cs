using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SeriesController : Controller
    {
        private readonly QynFigureContext _context;

        public SeriesController(QynFigureContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View(_context.Series.ToList());
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(SeriesModel series)
        {
            if (ModelState.IsValid)
            {
                _context.Series.Add(series);
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
        public async Task<IActionResult> Edit(int SeriesId)
        {
            var series = await _context.Series.FindAsync(SeriesId);
            return View(series);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int SeriesId, SeriesModel series)
        {
            var updateSeries = await _context.Series.FindAsync(series);

            if (updateSeries == null) // Add this check
            {
                TempData["error"] = "Không tìm dòng sản phẩm này";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                updateSeries.Name = series.Name;
                updateSeries.Description = series.Description;

                _context.Series.Update(series);
                await _context.SaveChangesAsync();
                TempData["success"] = "Chỉnh sửa loại sản phẩm thành công";
                return RedirectToAction("Index");
            }
            else
            {
                // Return to the Edit view with the model to show validation errors
                return View(series);
            }
        }

        public async Task<IActionResult> Remove(int SeriesId)
        {
            SeriesModel series = await _context.Series.FindAsync(SeriesId);
            _context.Series.Remove(series);
            _context.SaveChanges();
            TempData["error"] = "Xóa dòng sản phẩm thành công";
            return RedirectToAction("Index");
        }
    }
}
