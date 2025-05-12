using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models.ViewModels
{
    public class ProductDetailsViewModel
    {

        public ProductModel Product { get; set; }
        public List<ProductImageModel> ProductImages { get; set; } // Thêm danh sách ảnh

    }
}
