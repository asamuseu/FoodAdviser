using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodAdviser.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of receipt repository.
/// All queries are scoped to the specified user.
/// </summary>
public class ReceiptRepository : IReceiptRepository
{
    private readonly FoodAdviserDbContext _db;

    public ReceiptRepository(FoodAdviserDbContext db)
    {
        _db = db;
    }

    public async Task<Receipt?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        return await _db.Receipts
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, ct);
    }

    public async Task<Receipt> AddAsync(Receipt receipt, CancellationToken ct = default)
    {
        if (receipt.Id == Guid.Empty)
        {
            receipt.Id = Guid.NewGuid();
        }
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync(ct);
        return receipt;
    }

    public async Task<IReadOnlyList<Receipt>> GetRecentAsync(int count, Guid userId, CancellationToken ct = default)
    {
        return await _db.Receipts
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(Math.Max(0, count))
            .ToListAsync(ct);
    }
}
