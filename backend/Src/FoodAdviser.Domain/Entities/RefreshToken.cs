namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Entity representing a refresh token for JWT authentication.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The refresh token value.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// The user ID this refresh token belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// When the refresh token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the refresh token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the refresh token was revoked (if applicable).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for revoking the token.
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// IP address from which the token was created.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address from which the token was revoked.
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Token that replaced this token (if rotated).
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Checks if the refresh token is currently active.
    /// </summary>
    public bool IsActive => RevokedAt == null && !IsExpired;

    /// <summary>
    /// Checks if the refresh token has expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
