using qyn_figure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore ;
using Microsoft.EntityFrameworkCore;

namespace qyn_figure.Repository;

public partial class QynFigureContext : IdentityDbContext<AppUserModel>
{
    public QynFigureContext(DbContextOptions<QynFigureContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ShippingModel> Shippings { get; set; }
    public virtual DbSet<ContactInfo> ContactInfos { get; set; }
    public virtual DbSet<ProductImageModel> ProductImages { get; set; }
    public virtual DbSet<BrandModel> Brands { get; set; }
    public virtual DbSet<SliderModel> Sliders { get; set; } = null!;
    public virtual DbSet<RatingModel> Ratings { get; set; }
    public virtual DbSet<ProductModel> Products { get; set; }
    public virtual DbSet<CategoryModel> Categories { get; set; }
    public virtual DbSet<SeriesModel> Series { get; set; }
    public virtual DbSet<OrderModel> Orders { get; set; }
    public virtual DbSet<OrderDetailModel> OrderDetails { get; set; }

    
}