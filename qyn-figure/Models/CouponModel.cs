using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Hãy nhập tên của coupon")]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public DateTime DateStart { get; set; }
        [Required]
        public DateTime DateEnd { get; set; }

        [Required(ErrorMessage = "Hãy nhập số lượng")]
        public int Quantity { get; set; }
        public int Status { get; set; }
    }
}
