namespace FoodAdviser.Application.DTOs.Auth;

/// <summary>
/// Response DTO for successful authentication.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// The JWT access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// The refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in UTC.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The authenticated user's ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The authenticated user's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The authenticated user's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The authenticated user's last name.
    /// </summary>
    public string? LastName { get; set; }
}
