namespace FoodAdviser.Application.DTOs.Inventory;

/// <summary>
/// Food item read model for API responses.
/// </summary>
public class FoodItemDto
{
    /// <summary>Item id.</summary>
    public Guid Id { get; set; }
    /// <summary>Name.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Quantity.</summary>
    public decimal Quantity { get; set; }
    /// <summary>Unit.</summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>Expiration date, if any.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
