using FoodAdviser.Domain.Enums;

namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Represents a recipe suggestion based on ingredients.
/// </summary>
public class Recipe
{
    public Guid Id { get; set; }

    /// <summary>The ID of the user who owns this recipe.</summary>
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DishType DishType { get; set; } = DishType.Undefined;
    public List<Ingredient> Ingredients { get; set; } = new();
}

/// <summary>
/// Ingredient and required portion.
/// </summary>
public class Ingredient
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
}
