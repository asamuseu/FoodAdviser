namespace FoodAdviser.Application.Options;

/// <summary>
/// JWT authentication configuration options.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// The secret key used to sign JWT tokens.
    /// Should be at least 32 characters for HS256.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// The issuer of the JWT token.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience for the JWT token.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration time in days.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
