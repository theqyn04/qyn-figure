using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Repository.Components
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly QynFigureContext _context;
        public CategoryViewComponent(QynFigureContext context)
        {
            _context = context;
        }
       
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }
    }
}
