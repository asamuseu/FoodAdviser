using System.Security.Claims;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FoodAdviser.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace FoodAdviser.Infrastructure.Tests.Services;

public class CurrentUserServiceTests
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly HttpContext _httpContext;
    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _httpContext = new DefaultHttpContext();
        _httpContextAccessor.HttpContext.Returns(_httpContext);
        
        _sut = new CurrentUserService(_httpContextAccessor);
    }

    // Note: Constructor doesn't validate null parameters since they're only accessed when properties are called

    [Fact]
    public void UserId_WithNoHttpContext_ReturnsNull()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessor);

        // Act
        var userId = service.UserId;

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void UserId_WithNoUser_ReturnsNull()
    {
        // Arrange
        _httpContext.User = null!;

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void UserId_WithUserIdClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("userId", expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void UserId_WithNameIdentifierClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void UserId_WithSubClaim_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("sub", expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void UserId_WithMultipleClaims_PrefersUserIdClaim()
    {
        // Arrange
        var userIdValue = Guid.NewGuid();
        var nameIdentifierValue = Guid.NewGuid();
        var subValue = Guid.NewGuid();
        
        var claims = new List<Claim>
        {
            new("userId", userIdValue.ToString()),
            new(ClaimTypes.NameIdentifier, nameIdentifierValue.ToString()),
            new("sub", subValue.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Equal(userIdValue, userId);
    }

    [Fact]
    public void UserId_WithInvalidGuid_ReturnsNull()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("userId", "not-a-valid-guid")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void UserId_WithEmptyGuid_ReturnsEmptyGuid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("userId", Guid.Empty.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Equal(Guid.Empty, userId);
    }

    [Fact]
    public void UserId_WithNoClaims_ReturnsNull()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.UserId;

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void GetRequiredUserId_WithValidUserId_ReturnsUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("userId", expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId = _sut.GetRequiredUserId();

        // Assert
        Assert.Equal(expectedUserId, userId);
    }

    [Fact]
    public void GetRequiredUserId_WithNoUserId_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => _sut.GetRequiredUserId());
        Assert.Equal("User is not authenticated.", exception.Message);
    }

    [Fact]
    public void GetRequiredUserId_WithNoHttpContext_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessor);

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(() => service.GetRequiredUserId());
        Assert.Equal("User is not authenticated.", exception.Message);
    }

    [Fact]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("userId", Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var isAuthenticated = _sut.IsAuthenticated;

        // Assert
        Assert.True(isAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_WithUnauthenticatedUser_ReturnsFalse()
    {
        // Arrange
        var claims = new List<Claim>();
        var identity = new ClaimsIdentity(claims); // No authentication type
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var isAuthenticated = _sut.IsAuthenticated;

        // Assert
        Assert.False(isAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_WithNoUser_ReturnsFalse()
    {
        // Arrange
        _httpContext.User = null!;

        // Act
        var isAuthenticated = _sut.IsAuthenticated;

        // Assert
        Assert.False(isAuthenticated);
    }

    [Fact]
    public void IsAuthenticated_WithNoHttpContext_ReturnsFalse()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        var service = new CurrentUserService(_httpContextAccessor);

        // Act
        var isAuthenticated = service.IsAuthenticated;

        // Assert
        Assert.False(isAuthenticated);
    }

    [Theory]
    [InlineData("TestAuth")]
    [InlineData("Bearer")]
    [InlineData("Cookie")]
    [InlineData("JWT")]
    public void IsAuthenticated_WithDifferentAuthTypes_ReturnsTrue(string authType)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("userId", Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, authType);
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var isAuthenticated = _sut.IsAuthenticated;

        // Assert
        Assert.True(isAuthenticated);
    }

    [Fact]
    public void UserId_CalledMultipleTimes_ReturnsConsistentValue()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("userId", expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var userId1 = _sut.UserId;
        var userId2 = _sut.UserId;
        var userId3 = _sut.UserId;

        // Assert
        Assert.Equal(expectedUserId, userId1);
        Assert.Equal(expectedUserId, userId2);
        Assert.Equal(expectedUserId, userId3);
    }

    [Fact]
    public void IsAuthenticated_CalledMultipleTimes_ReturnsConsistentValue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new("userId", Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _httpContext.User = new ClaimsPrincipal(identity);

        // Act
        var isAuth1 = _sut.IsAuthenticated;
        var isAuth2 = _sut.IsAuthenticated;
        var isAuth3 = _sut.IsAuthenticated;

        // Assert
        Assert.True(isAuth1);
        Assert.True(isAuth2);
        Assert.True(isAuth3);
    }
}





