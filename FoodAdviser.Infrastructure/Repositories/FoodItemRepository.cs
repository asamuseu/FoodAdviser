using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodAdviser.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFoodItemRepository.
/// </summary>
public class FoodItemRepository : IFoodItemRepository
{
    private readonly FoodAdviserDbContext _db;
    public FoodItemRepository(FoodAdviserDbContext db) => _db = db;

    public async Task<FoodItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.FoodItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<FoodItem>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
        => await _db.FoodItems.AsNoTracking()
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<FoodItem> AddAsync(FoodItem item, CancellationToken ct = default)
    {
        _db.FoodItems.Add(item);
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task UpdateAsync(FoodItem item, CancellationToken ct = default)
    {
        _db.FoodItems.Update(item);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.FoodItems.FindAsync(new object[] { id }, ct);
        if (entity is null) return;
        _db.FoodItems.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }
}
