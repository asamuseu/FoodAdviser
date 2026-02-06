using FoodAdviser.Application.DTOs.Recipes;

namespace FoodAdviser.Application.Services.Interfaces;

/// <summary>
/// Service interface for managing user inventory operations.
/// All operations are scoped to the current authenticated user.
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Confirms the preparation of specified recipes and updates the inventory accordingly.
    /// </summary>
    /// <param name="recipeIds">Collection of recipe identifiers to confirm.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A response containing the result of the confirmation and inventory updates.</returns>
    /// <exception cref="InvalidOperationException">Thrown when recipes are not found or inventory is insufficient.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated.</exception>
    Task<ConfirmRecipesResponseDto> ConfirmRecipesAsync(
        IReadOnlyList<Guid> recipeIds,
        CancellationToken ct = default);
}
