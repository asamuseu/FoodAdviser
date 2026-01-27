using System.ComponentModel.DataAnnotations;
using FoodAdviser.Application.DTOs.Receipts;

namespace FoodAdviser.Application.DTOs.Receipts;

/// <summary>
/// Receipt response DTO for API contracts.
/// </summary>
public class ReceiptDto
{
    /// <summary>
    /// Unique identifier of the receipt.
    /// </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary>
    /// UTC timestamp when the receipt was created or analyzed.
    /// </summary>
    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Optional human-readable description or store name.
    /// </summary>
    [MaxLength(256)]
    public string? Description { get; set; }

    /// <summary>
    /// Line items in the receipt.
    /// </summary>
    [Required]
    public IReadOnlyList<ReceiptLineItemDto> Items { get; set; } = Array.Empty<ReceiptLineItemDto>();

    /// <summary>
    /// Total price across all items.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Total { get; set; }
}
