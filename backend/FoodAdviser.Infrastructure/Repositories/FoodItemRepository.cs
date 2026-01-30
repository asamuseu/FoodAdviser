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

    /// <inheritdoc />
    public async Task<FoodItem?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _db.FoodItems.FirstOrDefaultAsync(
            x => x.Name.ToLower() == name.ToLower(), ct);

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

    /// <inheritdoc />
    public async Task<IReadOnlyList<FoodItem>> GetAvailableItemsAsync(CancellationToken ct = default)
        => await _db.FoodItems
            .AsNoTracking()
            .Where(x => x.Quantity > 0)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<FoodItem>> GetByNamesAsync(IEnumerable<string> names, CancellationToken ct = default)
    {
        var nameList = names.Select(n => n.ToLower()).ToList();
        return await _db.FoodItems
            .Where(x => nameList.Contains(x.Name.ToLower()))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateRangeAsync(IEnumerable<FoodItem> items, CancellationToken ct = default)
    {
        _db.FoodItems.UpdateRange(items);
        await _db.SaveChangesAsync(ct);
    }
}
