using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service for generating and validating JWT tokens.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <param name="roles">The user's roles.</param>
    /// <returns>The generated JWT token.</returns>
    string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>A secure random refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Gets the token expiration time.
    /// </summary>
    /// <returns>The UTC datetime when the token expires.</returns>
    DateTime GetAccessTokenExpiration();
}
