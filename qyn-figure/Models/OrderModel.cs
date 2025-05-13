using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Models;

public partial class OrderModel
{
    [Key]
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = null!;

    public string CustomerId { get; set; }
    public string? CouponCode { get; set; }

    public DateTime OrderDate { get; set; }
    [Precision(18, 2)]
    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;


    public virtual ICollection<OrderDetailModel> OrderDetails { get; set; } = new List<OrderDetailModel>();
}
