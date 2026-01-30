using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository abstraction for receipts.
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default);
    Task<IReadOnlyList<Receipt>> GetRecentAsync(int count, CancellationToken ct = default);
}
