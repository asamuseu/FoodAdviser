using FoodAdviser.Application.DTOs.Auth;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service for handling authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>The authentication response if successful, or null if registration failed.</returns>
    Task<(AuthResponseDto? Response, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDto request);

    /// <summary>
    /// Authenticates a user and returns tokens.
    /// </summary>
    /// <param name="request">The login request.</param>
    /// <returns>The authentication response if successful, or null if login failed.</returns>
    Task<(AuthResponseDto? Response, string? Error)> LoginAsync(LoginRequestDto request);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>The new authentication response if successful, or null if refresh failed.</returns>
    Task<(AuthResponseDto? Response, string? Error)> RefreshTokenAsync(string refreshToken);
}
