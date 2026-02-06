using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for receipts.
/// All operations are scoped to a specific user.
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default);
    Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default);
    Task<IReadOnlyList<Receipt>> GetRecentAsync(int count, Guid userId, CancellationToken ct = default);
}
