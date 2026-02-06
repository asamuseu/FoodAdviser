namespace FoodAdviser.Application.DTOs.Recipes;

/// <summary>
/// DTO representing an ingredient with its required quantity.
/// </summary>
public class IngredientDto
{
    /// <summary>
    /// The name of the ingredient.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The required quantity of the ingredient.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// The unit of measurement (e.g., g, ml, pcs).
    /// </summary>
    public string Unit { get; set; } = string.Empty;
}
