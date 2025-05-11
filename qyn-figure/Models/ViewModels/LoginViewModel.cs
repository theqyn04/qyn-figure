using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username là bắt buộc")]
        [Display(Name = "Username hoặc Email")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password là bắt buộc")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; } // Cho phép null
    }
}
