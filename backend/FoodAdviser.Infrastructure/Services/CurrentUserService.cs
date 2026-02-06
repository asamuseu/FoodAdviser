using System.Security.Claims;
using FoodAdviser.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// Implementation of ICurrentUserService that retrieves user information from JWT claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }

    /// <inheritdoc />
    public Guid GetRequiredUserId()
    {
        var userId = UserId;
        if (userId == null)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId.Value;
    }

    /// <inheritdoc />
    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
