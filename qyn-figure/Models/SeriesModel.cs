using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models
{
    public class SeriesModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Hãy nhập tên loại sản phẩm!")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public virtual ICollection<ProductModel> Product { get; set; } = new List<ProductModel>();
    }
}
