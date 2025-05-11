namespace qyn_figure.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartModel> CartItems { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
