using Microsoft.AspNetCore.Mvc;
using qyn_figure.Repository;

namespace qyn_figure.Controllers
{
    public class ContactInfoController : Controller
    {
        private readonly QynFigureContext _context;
        public ContactInfoController(QynFigureContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var info = _context.ContactInfos.FirstOrDefault();
            return View(info);
        }
    }
}
