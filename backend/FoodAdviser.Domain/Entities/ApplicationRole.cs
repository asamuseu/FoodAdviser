using Microsoft.AspNetCore.Identity;

namespace FoodAdviser.Domain.Entities;

/// <summary>
/// Custom application role entity with GUID as the primary key.
/// </summary>
public class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Description of the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ApplicationRole() : base()
    {
    }

    /// <summary>
    /// Constructor with role name.
    /// </summary>
    /// <param name="roleName">The role name.</param>
    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}
