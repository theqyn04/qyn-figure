using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using qyn_figure.Repository.Validation;

namespace qyn_figure.Models
{

    public class SliderModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hãy nhập tiêu đề")]
        public string Name { get; set; }

        public string Description { get; set; }
        public string? Image { get; set; }
        public int? Status { get; set; }
        public string? CreatedAt { get; set; }

        [NotMapped]
        [FileExtension(ErrorMessage = "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png, .gif")]
        [Required(ErrorMessage = "Vui lòng chọn ảnh slider")]
        public IFormFile ImageFile { get; set; }
    }
}
