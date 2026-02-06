namespace FoodAdviser.Api.DTOs.Auth;

/// <summary>
/// DTO for current user information.
/// </summary>
public class UserInfoDto
{
    /// <summary>
    /// The user's ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The user's email.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The user's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// The user's last name.
    /// </summary>
    public string? LastName { get; set; }
}