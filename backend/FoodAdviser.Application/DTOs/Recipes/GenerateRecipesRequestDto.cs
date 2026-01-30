using FoodAdviser.Domain.Enums;

namespace FoodAdviser.Application.DTOs.Recipes;

/// <summary>
/// Request DTO for generating recipe suggestions.
/// </summary>
public class GenerateRecipesRequestDto
{
    /// <summary>
    /// The type of dish to generate recipes for.
    /// </summary>
    public DishType DishType { get; set; }

    /// <summary>
    /// The number of persons the recipes should serve.
    /// </summary>
    public int NumberOfPersons { get; set; }
}
