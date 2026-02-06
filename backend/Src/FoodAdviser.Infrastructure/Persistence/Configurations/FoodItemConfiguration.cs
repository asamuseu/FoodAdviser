using FoodAdviser.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodAdviser.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for FoodItem entity.
/// </summary>
public class FoodItemConfiguration : IEntityTypeConfiguration<FoodItem>
{
    public void Configure(EntityTypeBuilder<FoodItem> builder)
    {
        builder.ToTable("food_items");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();
        builder.Property(f => f.Name).HasMaxLength(256).IsRequired();
        builder.Property(f => f.Quantity).HasColumnType("numeric(18,4)");
        builder.Property(f => f.Unit).HasMaxLength(64).IsRequired();
        builder.Property(f => f.ExpiresAt);

        // Unique constraint on Name
        builder.HasIndex(f => f.Name).IsUnique();
    }
}
