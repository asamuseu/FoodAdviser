using FoodAdviser.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodAdviser.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Recipe entity.
/// </summary>
public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Title).HasMaxLength(256).IsRequired();

        builder.OwnsMany(r => r.Ingredients, ingredientsBuilder =>
        {
            ingredientsBuilder.ToTable("recipe_ingredients");
            ingredientsBuilder.WithOwner().HasForeignKey("RecipeId");
            ingredientsBuilder.HasKey("RecipeId", "Name");
            ingredientsBuilder.Property(i => i.Name).HasMaxLength(256).IsRequired();
            ingredientsBuilder.Property(i => i.Unit).HasMaxLength(64).IsRequired();
            ingredientsBuilder.Property(i => i.Quantity).HasColumnType("numeric(18,4)");
            ingredientsBuilder.Property<Guid>("RecipeId");
        });
    }
}
