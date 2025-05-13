namespace qyn_figure.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartModel> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
        public string CouponCode { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal => TotalPrice - Discount;
    }
}
