using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Models
{
    public class UserModel
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required(ErrorMessage = "Hãy nhập username!")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Hãy nhập password!")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Hãy nhập email!"), EmailAddress]
        public string Email { get; set; }

        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số điện thoại chỉ được chứa số")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }
    }
}
