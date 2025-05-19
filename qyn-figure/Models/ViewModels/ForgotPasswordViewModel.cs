using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
