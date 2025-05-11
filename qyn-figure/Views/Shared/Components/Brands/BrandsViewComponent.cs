using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Repository;

namespace qyn_figure.Views.Shared.Components.Brands
{
    // Đặt tại: /Components/BrandsViewComponent.cs
    public class BrandsViewComponent : ViewComponent
    {
        private readonly QynFigureContext _context;

        public BrandsViewComponent(QynFigureContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var brands = await _context.Brands.ToListAsync();
            return View("Default", brands);
        }
    }
}
