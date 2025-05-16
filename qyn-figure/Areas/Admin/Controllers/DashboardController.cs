using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qyn_figure.Repository;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly QynFigureContext _context;
        public DashboardController(QynFigureContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var orderCount = _context.Orders.Count();
            var userCount = _context.Users.Count();
            var productCount = _context.Products.Count();
            var brandCount = _context.Brands.Count();
            ViewBag.OrderCount = orderCount;
            ViewBag.UserCount = userCount;
            ViewBag.ProductCount = productCount;
            ViewBag.BrandCount = brandCount;
            return View();
        }
    }
}
