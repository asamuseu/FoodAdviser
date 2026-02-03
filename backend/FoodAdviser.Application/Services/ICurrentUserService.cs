namespace FoodAdviser.Application.Services;

/// <summary>
/// Service for accessing the current authenticated user's information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current authenticated user's ID.
    /// </summary>
    /// <returns>The user's GUID, or null if not authenticated.</returns>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current authenticated user's ID, throwing if not authenticated.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated.</exception>
    Guid GetRequiredUserId();

    /// <summary>
    /// Gets whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }
}
