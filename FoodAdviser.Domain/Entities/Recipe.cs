namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Represents a recipe suggestion based on ingredients.
/// </summary>
public class Recipe
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
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
