using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using qyn_figure.Models;

namespace qyn_figure.Models;

public partial class CategoryModel
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Hãy nhập tên loại sản phẩm!")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<ProductModel> Product { get; set; } = new List<ProductModel>();
}
