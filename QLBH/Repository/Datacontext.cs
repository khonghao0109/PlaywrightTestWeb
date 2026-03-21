using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyWebApp.Models;
using Microsoft.AspNetCore.Identity;

namespace MyWebApp.Repository
{
    public class DataContext : IdentityDbContext<AppUserModel, IdentityRole, string>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<BrandModel> Brands { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<StatisticalModel> Statistical { get; set; }
        public DbSet<OrderAddress> OrderAddresses { get; set; }

        public DbSet<VietQRPaymentNotification> VietQRPaymentNotifications { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            
            base.OnModelCreating(builder);

            builder.Entity<ProductModel>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<OrderDetails>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<VietQRPaymentNotification>().HasNoKey();
            builder.Entity<AppUserModel>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Rất quan trọng
                
        }
    }
}