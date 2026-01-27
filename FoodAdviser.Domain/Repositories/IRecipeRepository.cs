using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for recipes.
/// </summary>
public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Recipe> AddAsync(Recipe recipe, CancellationToken ct = default);
    Task<IReadOnlyList<Recipe>> GetAllAsync(CancellationToken ct = default);
}
