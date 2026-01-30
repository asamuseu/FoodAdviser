using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for managing FoodItem entities.
/// </summary>
public interface IFoodItemRepository
{
    Task<FoodItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>
    /// Gets a food item by its name (case-insensitive).
    /// </summary>
    /// <param name="name">The name of the food item to find.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The food item if found; otherwise, null.</returns>
    Task<FoodItem?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<FoodItem>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<FoodItem> AddAsync(FoodItem item, CancellationToken ct = default);
    Task UpdateAsync(FoodItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets all food items with quantity greater than zero.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of available food items.</returns>
    Task<IReadOnlyList<FoodItem>> GetAvailableItemsAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets food items by their names (case-insensitive).
    /// </summary>
    /// <param name="names">Collection of food item names.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of food items matching the provided names.</returns>
    Task<IReadOnlyList<FoodItem>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct = default);

    /// <summary>
    /// Updates multiple food items in a single transaction.
    /// </summary>
    /// <param name="items">The food items to update.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRangeAsync(IEnumerable<FoodItem> items, CancellationToken ct = default);
}
