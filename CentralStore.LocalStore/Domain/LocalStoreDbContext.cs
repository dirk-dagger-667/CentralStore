using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace CentralStore.LocalStore.Domain
{
  public class LocalStoreDbContext : DbContext
  {
    public DbSet<Product> Products { get; set; }

    public LocalStoreDbContext(DbContextOptions<LocalStoreDbContext> options)
      : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<Product>(product => 
      {
        product.HasKey(p => p.Id);
        product.Property(p => p.Name).IsRequired().HasMaxLength(100);
        product.Property(p => p.Description).IsRequired().HasMaxLength(500);
        product.Property(p => p.Price).HasPrecision(10, 2);
        product.Property(p => p.MinPrice).HasPrecision(10, 2);
      });

      modelBuilder.AddInboxStateEntity();
      modelBuilder.AddOutboxMessageEntity();
      modelBuilder.AddOutboxStateEntity();
    }
  }
}
