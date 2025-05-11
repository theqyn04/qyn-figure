using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qyn_figure.Models
{
    public class ContactInfo
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tên công ty")]
        [Required(ErrorMessage = "Vui lòng nhập tên công ty")]
        public string CompanyName { get; set; }

        [Display(Name = "Logo")]
        public string? LogoPath { get; set; } // Đường dẫn đến file logo

        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Link Facebook")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string FacebookLink { get; set; }

        [Display(Name = "Link Zalo")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string ZaloLink { get; set; }

        [Display(Name = "Link YouTube")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string YoutubeLink { get; set; }

        [Display(Name = "Link Twitter")]
        [Url(ErrorMessage = "URL không hợp lệ")]
        public string TwitterLink { get; set; }

        [Display(Name = "Mã nhúng bản đồ")]
        public string MapEmbedCode { get; set; } // HTML iframe hoặc script

        [Display(Name = "Giờ làm việc")]
        public string WorkingHours { get; set; }

        [Display(Name = "Mô tả ngắn")]
        public string ShortDescription { get; set; }

        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
    }
}