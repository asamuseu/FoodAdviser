using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Domain.Repositories;

/// <summary>
/// Repository interface for refresh token operations.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Gets a refresh token by its token value.
    /// </summary>
    /// <param name="token">The token value to search for.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets all active refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A collection of active refresh tokens.</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);

    /// <summary>
    /// Adds a new refresh token to the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    Task AddAsync(RefreshToken refreshToken);

    /// <summary>
    /// Updates an existing refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to update.</param>
    Task UpdateAsync(RefreshToken refreshToken);

    /// <summary>
    /// Revokes all active refresh tokens for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="revokedByIp">The IP address revoking the tokens.</param>
    Task RevokeAllUserTokensAsync(Guid userId, string reason, string? revokedByIp = null);

    /// <summary>
    /// Removes expired refresh tokens from the database.
    /// </summary>
    /// <returns>The number of tokens removed.</returns>
    Task<int> RemoveExpiredTokensAsync();
}
