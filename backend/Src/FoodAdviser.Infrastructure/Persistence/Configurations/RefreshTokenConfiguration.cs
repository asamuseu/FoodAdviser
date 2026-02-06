using FoodAdviser.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodAdviser.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the RefreshToken entity.
/// </summary>
public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(rt => rt.RevokedByIp)
            .HasMaxLength(45);

        builder.Property(rt => rt.RevokedReason)
            .HasMaxLength(500);

        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(500);

        // Indexes for performance
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresAt);
    }
}
