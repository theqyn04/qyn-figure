using System.ComponentModel.DataAnnotations;

namespace qyn_figure.Repository.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                string[] allowedExtensions = { "jpg", "png", "jpeg" };

                bool result = allowedExtensions.Any(x => extension.EndsWith(x));

                if (!result)
                {
                    return new ValidationResult("Chỉ chấp nhận file ảnh có đuôi .jpg, .png hoặc .jpeg");
                }
            }
            return ValidationResult.Success;
        }
    }
}