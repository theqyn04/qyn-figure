using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace qyn_figure.Models
{
    public class RatingModel
    {
        [Key]
        public int Id { get; set; }

        public int ProductId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên của bạn!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá!")]
        public string Comment { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn số sao!")]
        public int Star { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "Vui lòng nhập email của bạn!")]
        public string Email { get; set; }

        [ForeignKey("Id")]
        public virtual ProductModel product { get; set; }

    }
}
