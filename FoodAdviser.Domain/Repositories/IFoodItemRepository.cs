using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for managing FoodItem entities.
/// </summary>
public interface IFoodItemRepository
{
    Task<FoodItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FoodItem>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<FoodItem> AddAsync(FoodItem item, CancellationToken ct = default);
    Task UpdateAsync(FoodItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
