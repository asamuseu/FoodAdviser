using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Enums;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service interface for OpenAI integration to generate recipe suggestions.
/// </summary>
public interface IOpenAiService
{
    /// <summary>
    /// Generates recipe suggestions based on available food items using OpenAI.
    /// </summary>
    /// <param name="availableItems">List of available food items with quantities.</param>
    /// <param name="dishType">The type of dish to generate.</param>
    /// <param name="numberOfPersons">Number of persons the recipe should serve.</param>
    /// <param name="recipeCount">Number of recipe suggestions to generate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of generated recipes, or empty list if none could be generated.</returns>
    Task<IReadOnlyList<Recipe>> GenerateRecipesAsync(
        IReadOnlyList<FoodItem> availableItems,
        DishType dishType,
        int numberOfPersons,
        int recipeCount,
        CancellationToken ct = default);
}
