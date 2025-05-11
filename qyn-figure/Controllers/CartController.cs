using qyn_figure.Repository;
using Microsoft.AspNetCore.Mvc;
using qyn_figure.Models;
using qyn_figure.Models.ViewModels;

namespace qyn_figure.Controllers
{
    public class CartController : Controller
    {
        private QynFigureContext _context;
        public CartController(QynFigureContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<CartModel> cartItem = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();
            CartViewModel cartVM = new CartViewModel()
            {
                CartItems = cartItem,
                TotalPrice = cartItem.Sum(x => x.Quantity * x.Price)
            };
            return View(cartVM);
        }

        public IActionResult Checkout()
        {
            return View("~/Views/Checkout/Index.cshtml");
        }

        public async Task<IActionResult> AddToCart(int Id)
        {
            ProductModel product = await _context.Products.FindAsync(Id);
            if (product != null)
            {
                List<CartModel> cartItem = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();
                CartModel item = cartItem.FirstOrDefault(x => x.ProductId == Id);
                if (item == null)
                {
                    cartItem.Add(new CartModel(product));
                }
                else
                {
                    item.Quantity++;
                }
                HttpContext.Session.SetJson("cart", cartItem);
            }

            TempData["Success"] = "Thêm vào giỏ hàng thành công!";
            return Redirect(Request.Headers["Referer"].ToString());
        }

        public async Task<IActionResult> Increase(int Id)
        {
            List<CartModel> cart = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();
            CartModel item = cart.FirstOrDefault(x => x.ProductId == Id);

            if (item != null)
            {
                item.Quantity++; // Luôn tăng số lượng bất kể giá trị hiện tại
                HttpContext.Session.SetJson("cart", cart);
            }

            return RedirectToAction("Index");
        }

        // Thêm action giảm số lượng
        public async Task<IActionResult> Decrease(int Id)
        {
            List<CartModel> cart = HttpContext.Session.GetJson<List<CartModel>>("cart");
            CartModel item = cart.FirstOrDefault(x => x.ProductId == Id);

            if (item != null)
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cart.Remove(item);
                }

                if (cart.Count > 0)
                {
                    HttpContext.Session.SetJson("cart", cart);
                }
                else
                {
                    HttpContext.Session.Remove("cart");
                }
            }

            return RedirectToAction("Index");
        }
    }
}
