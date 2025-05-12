using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Models;

public partial class ProductModel
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên!")]
    public string Name { get; set; } = null!;   

    public int? CategoryId { get; set; }

    public int? BrandId { get; set; }

    public int? SeriesId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm!")]
    [Range(0, 10000000000, ErrorMessage = "Giá sản phẩm không hợp lệ!")]
    [Precision(18, 2)]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập số lượng")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "Chỉ được nhập số")]
    public int StockQuantity { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    [NotMapped]
    public IFormFile? ImageUpload { get; set; }

    public CategoryModel? Category { get; set; }

    public BrandModel? Brand { get; set; }

    public SeriesModel? Series { get; set; }

    public virtual ICollection<OrderDetailModel> OrderDetails { get; set; } = new List<OrderDetailModel>();

    public virtual ICollection<ProductImageModel> ProductImages { get; set; } = new List<ProductImageModel>();


}
