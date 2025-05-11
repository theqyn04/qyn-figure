using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Models;

public partial class OrderDetailModel
{
    [Key]
    public int OrderDetailId { get; set; }

    public string OrderCode { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }
    [Precision(18, 2)]
    public decimal UnitPrice { get; set; }
    [Precision(18, 2)]
    public decimal Subtotal { get; set; }

    public virtual ProductModel? product { get; set; }

    public virtual OrderModel? Order { get; set; }
}
