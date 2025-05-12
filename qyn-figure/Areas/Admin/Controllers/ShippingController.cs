using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using qyn_figure.Models;
using qyn_figure.Repository;

namespace qyn_figure.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ShippingController : Controller
    {
        private readonly QynFigureContext _context;
        public ShippingController(QynFigureContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StoreShipping(ShippingModel shipping, string phuong, string quan, string tinh, double price)
        {
            shipping.Ward = phuong;
            shipping.District = quan;
            shipping.City = tinh;
            shipping.Price = price;

            try
            {
                var existingShipping = await _context.Shippings.AnyAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
                if (existingShipping)
                {
                    return Ok(new { duplicate = true, message = "Dữ liệu trùng lặp." });
                }
                _context.Shippings.Add(shipping);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Thêm shipping thành công" });
            }
            catch (Exception) 
            {
                return StatusCode(500, "An error occurred while adding shipping");
            }
        }
    }
}
