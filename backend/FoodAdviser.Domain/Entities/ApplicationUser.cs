using Microsoft.AspNetCore.Identity;

namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Custom application user entity with GUID as the primary key.
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Date when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the user was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }
}
