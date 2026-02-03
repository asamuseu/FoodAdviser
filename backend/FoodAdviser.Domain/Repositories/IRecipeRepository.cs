using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for recipes.
/// All operations are scoped to a specific user.
/// </summary>
public interface IRecipeRepository
{
    /// <summary>
    /// Gets a recipe by its identifier for a specific user.
    /// </summary>
    Task<Recipe?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a new recipe to the database.
    /// </summary>
    Task<Recipe> AddAsync(Recipe recipe, CancellationToken ct = default);

    /// <summary>
    /// Gets all recipes for a specific user.
    /// </summary>
    Task<IReadOnlyList<Recipe>> GetAllAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds multiple recipes to the database in a single transaction.
    /// </summary>
    /// <param name="recipes">The recipes to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The added recipes with their generated identifiers.</returns>
    Task<IReadOnlyList<Recipe>> AddRangeAsync(IEnumerable<Recipe> recipes, CancellationToken ct = default);

    /// <summary>
    /// Gets recipes by their identifiers for a specific user.
    /// </summary>
    /// <param name="ids">Collection of recipe identifiers.</param>
    /// <param name="userId">The user ID to scope the query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of recipes matching the provided identifiers.</returns>
    Task<IReadOnlyList<Recipe>> GetByIdsAsync(IEnumerable<Guid> ids, Guid userId, CancellationToken ct = default);
}
