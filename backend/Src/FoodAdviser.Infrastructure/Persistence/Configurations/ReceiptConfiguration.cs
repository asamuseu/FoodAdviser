using FoodAdviser.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodAdviser.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Receipt aggregate and line items.
/// </summary>
public class ReceiptConfiguration : IEntityTypeConfiguration<Receipt>
{
    public void Configure(EntityTypeBuilder<Receipt> builder)
    {
        builder.ToTable("receipts");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.OwnsMany(r => r.Items, itemsBuilder =>
        {
            itemsBuilder.ToTable("receipt_line_items");
            itemsBuilder.WithOwner().HasForeignKey("ReceiptId");
            itemsBuilder.HasKey(li => li.Id);
            itemsBuilder.Property(li => li.Id).ValueGeneratedNever();
            itemsBuilder.Property(li => li.Name).HasMaxLength(256).IsRequired();
            itemsBuilder.Property(li => li.Unit).HasMaxLength(64).IsRequired();
            itemsBuilder.Property(li => li.Quantity).HasColumnType("numeric(18,4)");
            itemsBuilder.Property(li => li.Price).HasColumnType("numeric(18,2)");
            itemsBuilder.Property<Guid>("ReceiptId");
        });
    }
}
