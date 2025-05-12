using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace qyn_figure.Models;

public class ProductImageModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    public string? ImageUrl { get; set; } = null!;

    public int? DisplayOrder { get; set; } = 0;

    public bool? IsDefault { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [ForeignKey("ProductId")]
    public ProductModel? Product { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}