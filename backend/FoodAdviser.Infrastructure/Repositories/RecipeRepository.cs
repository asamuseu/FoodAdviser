using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodAdviser.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IRecipeRepository.
/// All queries are scoped to the specified user.
/// </summary>
public class RecipeRepository : IRecipeRepository
{
    private readonly FoodAdviserDbContext _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeRepository"/> class.
    /// </summary>
    public RecipeRepository(FoodAdviserDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<Recipe?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct = default)
        => await _db.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<Recipe> AddAsync(Recipe recipe, CancellationToken ct = default)
    {
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync(ct);
        return recipe;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Recipe>> GetAllAsync(Guid userId, CancellationToken ct = default)
        => await _db.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Title)
            .ToListAsync(ct);

    /// <summary>
    /// Adds multiple recipes to the database in a single transaction.
    /// </summary>
    public async Task<IReadOnlyList<Recipe>> AddRangeAsync(IEnumerable<Recipe> recipes, CancellationToken ct = default)
    {
        var recipeList = recipes.ToList();
        _db.Recipes.AddRange(recipeList);
        await _db.SaveChangesAsync(ct);
        return recipeList;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Recipe>> GetByIdsAsync(IEnumerable<Guid> ids, Guid userId, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        return await _db.Recipes
            .AsNoTracking()
            .Include(r => r.Ingredients)
            .Where(r => r.UserId == userId && idList.Contains(r.Id))
            .ToListAsync(ct);
    }
}
