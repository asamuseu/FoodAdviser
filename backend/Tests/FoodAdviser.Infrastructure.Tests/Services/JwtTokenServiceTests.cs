using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoFixture.AutoNSubstitute;
using FoodAdviser.Application.Options;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Infrastructure.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoodAdviser.Infrastructure.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtOptions _jwtOptions;
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        _jwtOptions = new JwtOptions
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingPurposesOnly12345",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        var optionsWrapper = Substitute.For<IOptions<JwtOptions>>();
        optionsWrapper.Value.Returns(_jwtOptions);

        _sut = new JwtTokenService(optionsWrapper);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsNullReferenceException()
    {
        // Arrange
        IOptions<JwtOptions> nullOptions = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => new JwtTokenService(nullOptions));
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ReturnsValidToken()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var roles = new List<string> { "User" };

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        Assert.False(string.IsNullOrEmpty(token));

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal(_jwtOptions.Issuer, jwtToken.Issuer);
        Assert.Contains(jwtToken.Audiences, a => a == _jwtOptions.Audience);
        Assert.Contains(jwtToken.Claims, c => c.Type == "userId" && c.Value == user.Id.ToString());
        Assert.Contains(jwtToken.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        Assert.Contains(jwtToken.Claims, c => c.Type == "firstName" && c.Value == user.FirstName);
        Assert.Contains(jwtToken.Claims, c => c.Type == "lastName" && c.Value == user.LastName);
        Assert.Contains(jwtToken.Claims, c => c.Type == ClaimTypes.Role && c.Value == "User");
    }

    [Fact]
    public void GenerateAccessToken_WithMultipleRoles_IncludesAllRoles()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com"
        };
        var roles = new List<string> { "User", "Admin", "Manager" };

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
        Assert.Equal(3, roleClaims.Count);
        Assert.Contains("User", roleClaims);
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("Manager", roleClaims);
    }

    [Fact]
    public void GenerateAccessToken_WithNoRoles_GeneratesTokenWithoutRoleClaims()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "norole@example.com"
        };
        var roles = Array.Empty<string>();

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        Assert.Empty(roleClaims);
    }

    [Fact]
    public void GenerateAccessToken_WithNullFirstName_DoesNotIncludeFirstNameClaim()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = null,
            LastName = "Doe"
        };
        var roles = new List<string> { "User" };

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.DoesNotContain(jwtToken.Claims, c => c.Type == "firstName");
    }

    [Fact]
    public void GenerateAccessToken_WithEmptyLastName_DoesNotIncludeLastNameClaim()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = string.Empty
        };
        var roles = new List<string> { "User" };

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.DoesNotContain(jwtToken.Claims, c => c.Type == "lastName");
    }

    [Fact]
    public void GenerateAccessToken_HasExpirationTime()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var roles = new List<string> { "User" };

        // Act
        var token = _sut.GenerateAccessToken(user, roles);

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.True(jwtToken.ValidTo > DateTime.UtcNow);
        Assert.True(jwtToken.ValidTo <= DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes + 1));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _sut.GenerateRefreshToken();

        // Assert
        Assert.False(string.IsNullOrEmpty(refreshToken));
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesUniqueTokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();
        var token3 = _sut.GenerateRefreshToken();

        // Assert
        Assert.NotEqual(token1, token2);
        Assert.NotEqual(token2, token3);
        Assert.NotEqual(token1, token3);
    }

    [Fact]
    public void GenerateRefreshToken_GeneratesBase64String()
    {
        // Act
        var refreshToken = _sut.GenerateRefreshToken();

        // Assert
        // Should be able to convert from Base64 without exception
        var bytes = Convert.FromBase64String(refreshToken);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void GetAccessTokenExpiration_ReturnsCorrectExpiration()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;

        // Act
        var expiration = _sut.GetAccessTokenExpiration();

        // Assert
        var afterCall = DateTime.UtcNow;
        var expectedMin = beforeCall.AddMinutes(_jwtOptions.ExpirationMinutes);
        var expectedMax = afterCall.AddMinutes(_jwtOptions.ExpirationMinutes);

        Assert.InRange(expiration, expectedMin, expectedMax);
    }

    [Fact]
    public void GetRefreshTokenExpiration_ReturnsCorrectExpiration()
    {
        // Arrange
        var beforeCall = DateTime.UtcNow;

        // Act
        var expiration = _sut.GetRefreshTokenExpiration();

        // Assert
        var afterCall = DateTime.UtcNow;
        var expectedMin = beforeCall.AddDays(_jwtOptions.RefreshTokenExpirationDays);
        var expectedMax = afterCall.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        Assert.InRange(expiration, expectedMin, expectedMax);
    }

    [Fact]
    public void ValidateAccessToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var roles = new List<string> { "User" };
        var token = _sut.GenerateAccessToken(user, roles);

        // Act
        var principal = _sut.ValidateAccessToken(token);

        // Assert
        Assert.NotNull(principal);
        Assert.True(principal.Identity?.IsAuthenticated);
        Assert.Contains(principal.Claims, c => c.Type == "userId" && c.Value == user.Id.ToString());
    }

    [Fact]
    public void ValidateAccessToken_WithExpiredToken_CanStillValidate()
    {
        // Arrange - Create token with very short expiration
        var shortExpirationOptions = new JwtOptions
        {
            SecretKey = _jwtOptions.SecretKey,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            ExpirationMinutes = -1, // Expired
            RefreshTokenExpirationDays = 7
        };

        var optionsWrapper = Substitute.For<IOptions<JwtOptions>>();
        optionsWrapper.Value.Returns(shortExpirationOptions);
        var service = new JwtTokenService(optionsWrapper);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var token = service.GenerateAccessToken(user, new List<string>());

        // Act
        var principal = _sut.ValidateAccessToken(token);

        // Assert
        // Should still validate because ValidateLifetime is set to false in the service
        Assert.NotNull(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _sut.ValidateAccessToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithNullToken_ReturnsNull()
    {
        // Arrange
        string? nullToken = null;

        // Act
        var principal = _sut.ValidateAccessToken(nullToken!);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithEmptyToken_ReturnsNull()
    {
        // Arrange
        var emptyToken = string.Empty;

        // Act
        var principal = _sut.ValidateAccessToken(emptyToken);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithTokenFromDifferentIssuer_ReturnsNull()
    {
        // Arrange - Create token with different issuer
        var differentOptions = new JwtOptions
        {
            SecretKey = _jwtOptions.SecretKey,
            Issuer = "DifferentIssuer",
            Audience = _jwtOptions.Audience,
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        var optionsWrapper = Substitute.For<IOptions<JwtOptions>>();
        optionsWrapper.Value.Returns(differentOptions);
        var differentService = new JwtTokenService(optionsWrapper);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var token = differentService.GenerateAccessToken(user, new List<string>());

        // Act
        var principal = _sut.ValidateAccessToken(token);

        // Assert
        Assert.Null(principal);
    }

    [Fact]
    public void ValidateAccessToken_WithTokenFromDifferentSecret_ReturnsNull()
    {
        // Arrange - Create token with different secret
        var differentOptions = new JwtOptions
        {
            SecretKey = "DifferentSecretKeyThatIsLongEnoughForHmacSha256Testing",
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            ExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        var optionsWrapper = Substitute.For<IOptions<JwtOptions>>();
        optionsWrapper.Value.Returns(differentOptions);
        var differentService = new JwtTokenService(optionsWrapper);

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };
        var token = differentService.GenerateAccessToken(user, new List<string>());

        // Act
        var principal = _sut.ValidateAccessToken(token);

        // Assert
        Assert.Null(principal);
    }
}







