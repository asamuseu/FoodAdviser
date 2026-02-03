using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodAdviser.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFoodItemRepository.
/// All queries are scoped to the specified user.
/// </summary>
public class FoodItemRepository : IFoodItemRepository
{
    private readonly FoodAdviserDbContext _db;
    public FoodItemRepository(FoodAdviserDbContext db) => _db = db;

    public async Task<FoodItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => await _db.FoodItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<FoodItem?> GetByNameAsync(string name, Guid userId, CancellationToken ct = default)
        => await _db.FoodItems.FirstOrDefaultAsync(
            x => x.Name.ToLower() == name.ToLower() && x.UserId == userId, ct);

    public async Task<IReadOnlyList<FoodItem>> GetPagedAsync(int page, int pageSize, Guid userId, CancellationToken ct = default)
        => await _db.FoodItems.AsNoTracking()
            .Where(x => x.UserId == userId)
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

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var entity = await _db.FoodItems
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (entity is null) return;
        _db.FoodItems.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FoodItem>> GetAvailableItemsAsync(Guid userId, CancellationToken ct = default)
        => await _db.FoodItems
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Quantity > 0)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<FoodItem>> GetByNamesAsync(IEnumerable<string> names, Guid userId, CancellationToken ct = default)
    {
        var nameList = names.Select(n => n.ToLower()).ToList();
        return await _db.FoodItems
            .Where(x => x.UserId == userId && nameList.Contains(x.Name.ToLower()))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateRangeAsync(IEnumerable<FoodItem> items, CancellationToken ct = default)
    {
        _db.FoodItems.UpdateRange(items);
        await _db.SaveChangesAsync(ct);
    }
}
