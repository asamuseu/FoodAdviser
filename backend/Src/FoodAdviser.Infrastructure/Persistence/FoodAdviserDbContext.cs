using FoodAdviser.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FoodAdviser.Infrastructure.Persistence.Configurations;

namespace FoodAdviser.Infrastructure.Persistence;

/// <summary>
/// EF Core DbContext for FoodAdviser with Identity support.
/// Uses ApplicationUser and ApplicationRole with GUID as primary key.
/// </summary>
public class FoodAdviserDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
{
    public FoodAdviserDbContext(DbContextOptions<FoodAdviserDbContext> options) : base(options) { }

    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<Receipt> Receipts => Set<Receipt>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Call base to configure Identity tables
        base.OnModelCreating(modelBuilder);

        // Apply custom configurations
        modelBuilder.ApplyConfiguration(new FoodItemConfiguration());
        modelBuilder.ApplyConfiguration(new ReceiptConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Configure Identity table names (optional - customize as needed)
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("AspNetUsers");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<ApplicationRole>(entity =>
        {
            entity.ToTable("AspNetRoles");
            entity.Property(e => e.Description).HasMaxLength(500);
        });
    }
}
