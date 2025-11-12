using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TransformadoresApp.Models;

namespace TransformadoresApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Material> Materials => Set<Material>();
        public DbSet<UnitOfMeasure> UnitOfMeasures => Set<UnitOfMeasure>();
        public DbSet<BomItem> BomItems => Set<BomItem>();
        public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relación Material → UnitOfMeasure (evita borrado en cascada)
            modelBuilder.Entity<Material>()
                .HasOne(m => m.UnitOfMeasure)
                .WithMany()
                .HasForeignKey(m => m.UnitOfMeasureId)
                .OnDelete(DeleteBehavior.Restrict);

            // BomItem: combinación única Product + Material
            modelBuilder.Entity<BomItem>()
                .HasIndex(b => new { b.ProductId, b.MaterialId })
                .IsUnique();

            modelBuilder.Entity<BomItem>()
                .Property(b => b.QuantityPerUnit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductionOrder>()
                .Property(o => o.Quantity)
                .HasPrecision(18, 2);
        }
    }
}
