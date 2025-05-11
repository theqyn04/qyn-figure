using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Repository;

namespace qyn_figure.Views.Shared.Components.Categories
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly QynFigureContext _context;

        public CategoriesViewComponent(QynFigureContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories.ToListAsync(); // Thêm await
            return View("Default", categories);
        }
    }
}
