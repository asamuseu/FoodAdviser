using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Domain.Enums;

namespace FoodAdviser.Application.Services.Interfaces;

/// <summary>
/// Service interface for generating and managing recipe suggestions.
/// </summary>
public interface IRecipeSuggestionService
{
    /// <summary>
    /// Generates recipe suggestions based on available inventory for a specific dish type and number of persons.
    /// </summary>
    /// <param name="dishType">The type of dish to generate recipes for.</param>
    /// <param name="numberOfPersons">Number of persons the recipes should serve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of generated recipe DTOs.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no recipes could be generated.</exception>
    Task<IReadOnlyList<RecipeDto>> GenerateRecipeSuggestionsAsync(
        DishType dishType,
        int numberOfPersons,
        CancellationToken ct = default);
}
