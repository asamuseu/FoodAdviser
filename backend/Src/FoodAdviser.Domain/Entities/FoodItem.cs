namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Represents a food item in the inventory.
/// </summary>
public class FoodItem
{
    /// <summary>Primary identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>The ID of the user who owns this food item.</summary>
    public Guid UserId { get; set; }

    /// <summary>Name of the item.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Quantity available.</summary>
    public decimal Quantity { get; set; }
    /// <summary>Unit of measurement (e.g., g, ml, pcs).</summary>
    public string Unit { get; set; } = string.Empty;
    /// <summary>Optional expiration date.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
