using FoodAdviser.Application.Options;

namespace FoodAdviser.Application.Services.Interfaces;

/// <summary>
/// Factory interface for creating AI recipe services based on configuration.
/// </summary>
public interface IAiRecipeServiceFactory
{
    /// <summary>
    /// Gets the AI recipe service based on the configured active provider.
    /// </summary>
    /// <returns>The configured AI recipe service.</returns>
    IAiRecipeService GetService();

    /// <summary>
    /// Gets the AI recipe service for a specific provider.
    /// </summary>
    /// <param name="provider">The AI provider to use.</param>
    /// <returns>The AI recipe service for the specified provider.</returns>
    IAiRecipeService GetService(AiProvider provider);
}
