namespace FoodAdviser.Application.Options;

/// <summary>
/// Configuration options for recipe suggestion functionality.
/// </summary>
public class RecipeSuggestionOptions
{
    /// <summary>
    /// The default number of recipe suggestions to generate.
    /// </summary>
    public int DefaultRecipeCount { get; set; } = 3;

    /// <summary>
    /// Maximum number of recipe suggestions allowed per request.
    /// </summary>
    public int MaxRecipeCount { get; set; } = 10;
}
