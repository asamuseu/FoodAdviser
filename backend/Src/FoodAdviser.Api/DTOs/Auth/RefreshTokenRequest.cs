namespace FoodAdviser.Api.DTOs.Auth;

/// <summary>
/// Request DTO for refresh token.
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}