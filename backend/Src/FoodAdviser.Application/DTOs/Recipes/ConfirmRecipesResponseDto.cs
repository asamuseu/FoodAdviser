namespace FoodAdviser.Application.DTOs.Recipes;

/// <summary>
/// Response DTO for recipe confirmation results.
/// </summary>
public class ConfirmRecipesResponseDto
{
    /// <summary>
    /// Indicates whether all recipes were successfully confirmed.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message describing the result of the operation.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Number of recipes that were confirmed.
    /// </summary>
    public int ConfirmedRecipesCount { get; set; }

    /// <summary>
    /// List of inventory updates that were applied.
    /// </summary>
    public List<InventoryUpdateDto> InventoryUpdates { get; set; } = new();
}

/// <summary>
/// DTO representing an inventory update for a specific product.
/// </summary>
public class InventoryUpdateDto
{
    /// <summary>
    /// Name of the product.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity before the update.
    /// </summary>
    public decimal PreviousQuantity { get; set; }

    /// <summary>
    /// Quantity used from recipes.
    /// </summary>
    public decimal UsedQuantity { get; set; }

    /// <summary>
    /// Quantity after the update.
    /// </summary>
    public decimal NewQuantity { get; set; }

    /// <summary>
    /// Unit of measurement.
    /// </summary>
    public string Unit { get; set; } = string.Empty;
}
