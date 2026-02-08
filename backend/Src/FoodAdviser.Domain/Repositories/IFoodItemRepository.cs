using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for managing FoodItem entities.
/// All operations are scoped to a specific user.
/// </summary>
public interface IFoodItemRepository
{
    Task<FoodItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets a food item by its name (case-insensitive) for a specific user.
    /// </summary>
    /// <param name="name">The name of the food item to find.</param>
    /// <param name="userId">The user ID to scope the query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The food item if found; otherwise, null.</returns>
    Task<FoodItem?> GetByNameAsync(string name, Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<FoodItem>> GetPagedAsync(int page, int pageSize, Guid userId, CancellationToken ct = default);
    Task<FoodItem> AddAsync(FoodItem item, CancellationToken ct = default);
    Task UpdateAsync(FoodItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets all food items with quantity greater than zero for a specific user.
    /// </summary>
    /// <param name="userId">The user ID to scope the query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of available food items.</returns>
    Task<IReadOnlyList<FoodItem>> GetAvailableItemsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Gets food items by their names (case-insensitive) for a specific user.
    /// </summary>
    /// <param name="names">Collection of food item names.</param>
    /// <param name="userId">The user ID to scope the query.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of food items matching the provided names.</returns>
    Task<IReadOnlyList<FoodItem>> GetByNamesAsync(IEnumerable<string> names, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Updates multiple food items in a single transaction.
    /// </summary>
    /// <param name="items">The food items to update.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRangeAsync(IEnumerable<FoodItem> items, CancellationToken ct = default);
}
