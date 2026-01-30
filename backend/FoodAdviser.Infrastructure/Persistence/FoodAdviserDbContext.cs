using FoodAdviser.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FoodAdviser.Infrastructure.Persistence.Configurations;

namespace FoodAdviser.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for FoodAdviser.
/// </summary>
public class FoodAdviserDbContext : DbContext
{
    public FoodAdviserDbContext(DbContextOptions<FoodAdviserDbContext> options) : base(options) { }

    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Recipe> Recipes => Set<Recipe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new FoodItemConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
