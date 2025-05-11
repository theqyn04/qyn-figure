using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public ProductModel Product { get; set; }
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá!")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập email của bạn!")]
        public string Email { get; set; }
    }
}
