using System.ComponentModel.DataAnnotations;

namespace FoodAdviser.Application.DTOs.Inventory;

/// <summary>
/// Request payload to create a new food item.
/// </summary>
public class CreateFoodItemDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required]
    [StringLength(50)]
    public string Unit { get; set; } = string.Empty;

    public DateTimeOffset? ExpiresAt { get; set; }
}
