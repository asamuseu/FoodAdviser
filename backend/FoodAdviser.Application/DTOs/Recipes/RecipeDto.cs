using FoodAdviser.Domain.Enums;

namespace FoodAdviser.Application.DTOs.Recipes;

/// <summary>
/// DTO representing a recipe suggestion.
/// </summary>
public class RecipeDto
{
    /// <summary>
    /// Unique identifier of the recipe.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name/title of the recipe.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the recipe including cooking instructions.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The type of dish.
    /// </summary>
    public DishType DishType { get; set; }

    /// <summary>
    /// List of required ingredients with their quantities.
    /// </summary>
    public List<IngredientDto> Ingredients { get; set; } = new();
}
