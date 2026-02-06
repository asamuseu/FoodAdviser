using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodAdviser.Infrastructure.Repositories;

/// <summary>
/// Implementation of refresh token repository.
/// </summary>
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly FoodAdviserDbContext _db;

    public RefreshTokenRepository(FoodAdviserDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _db.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
    {
        return await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _db.RefreshTokens.AddAsync(refreshToken);
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _db.RefreshTokens.Update(refreshToken);
        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason, string? revokedByIp = null)
    {
        var activeTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedReason = reason;
            token.RevokedByIp = revokedByIp;
        }

        await _db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<int> RemoveExpiredTokensAsync()
    {
        var expiredTokens = await _db.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync();

        if (expiredTokens.Any())
        {
            _db.RefreshTokens.RemoveRange(expiredTokens);
            await _db.SaveChangesAsync();
        }

        return expiredTokens.Count;
    }
}
