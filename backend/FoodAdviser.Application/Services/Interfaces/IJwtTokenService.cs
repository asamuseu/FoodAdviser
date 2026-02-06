using FoodAdviser.Domain.Entities;
using System.Security.Claims;

namespace FoodAdviser.Application.Services.Interfaces;

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
    /// Gets the access token expiration time.
    /// </summary>
    /// <returns>The UTC datetime when the token expires.</returns>
    DateTime GetAccessTokenExpiration();

    /// <summary>
    /// Gets the refresh token expiration time.
    /// </summary>
    /// <returns>The UTC datetime when the refresh token expires.</returns>
    DateTime GetRefreshTokenExpiration();

    /// <summary>
    /// Validates an access token and returns its claims principal.
    /// </summary>
    /// <param name="token">The access token to validate.</param>
    /// <returns>The claims principal if valid, null otherwise.</returns>
    ClaimsPrincipal? ValidateAccessToken(string token);
}
