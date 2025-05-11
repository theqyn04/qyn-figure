using System.Security.Claims;
using qyn_figure.Areas.Admin.Repository;
using qyn_figure.Models;
using qyn_figure.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly QynFigureContext _context;
        private readonly IEmailSender _emailSender;
        public CheckoutController(QynFigureContext context, IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _context = context;
        }

        public async Task<IActionResult> Checkout()
        {
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                // Create new order
                OrderModel order = new OrderModel();
                order.CustomerId = customerId;
                order.OrderDate = DateTime.Now;
                order.Status = "Pending";
                order.OrderCode = Guid.NewGuid().ToString();
                order.TotalAmount = 0;

                // Get cart items from session
                List<CartModel> cartItems = HttpContext.Session.GetJson<List<CartModel>>("cart") ?? new List<CartModel>();

                // Calculate total amount and prepare order details
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetailModel>();

                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        var subtotal = product.Price * item.Quantity;

                        orderDetails.Add(new OrderDetailModel
                        {
                            OrderCode = order.OrderCode,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = product.Price,
                            Subtotal = subtotal
                        });

                        totalAmount += subtotal;
                    }
                }

                // Update order total
                order.TotalAmount = totalAmount;

                // Add order to context
                _context.Orders.Add(order);

                // Add all order details
                _context.OrderDetails.AddRange(orderDetails);

                // Save changes
                await _context.SaveChangesAsync();

                // Clear the cart
                HttpContext.Session.Remove("cart");

                //Send confirmation email
                var receiver = "quyennn3pp1@gmail.com";
                var subject = "Xác nhận đơn hàng";
                var message = $"Cảm ơn bạn đã đặt hàng tại BookShop. Mã đơn hàng của bạn là: {order.OrderCode}. Tổng tiền: {totalAmount} VNĐ.";

                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng của bạn là: " + order.OrderCode;

                return RedirectToAction("Index", "Home");
            }
        }
    }
}
