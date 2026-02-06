using System.ComponentModel.DataAnnotations;

namespace FoodAdviser.Application.DTOs.Receipts;

/// <summary>
/// Line item DTO within a receipt.
/// </summary>
public class ReceiptLineItemDto
{
    /// <summary>
    /// Name of the purchased item.
    /// </summary>
    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Quantity purchased.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit of measurement (e.g., kg, pcs).
    /// </summary>
    [MaxLength(32)]
    public string? Unit { get; set; }

    /// <summary>
    /// Price of the item.
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}
