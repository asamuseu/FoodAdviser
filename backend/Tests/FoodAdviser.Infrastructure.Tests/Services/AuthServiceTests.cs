using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FoodAdviser.Application.DTOs.Auth;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace FoodAdviser.Infrastructure.Tests.Services;

public class AuthServiceTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        // Mock UserManager
        var userStore = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            userStore, null, null, null, null, null, null, null, null);
        
        // Mock SignInManager
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager, contextAccessor, claimsFactory, null, null, null, null);
        
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        
        // Setup default HTTP context
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        _httpContextAccessor.HttpContext.Returns(httpContext);
        
        _sut = new AuthService(
            _userManager, 
            _signInManager, 
            _jwtTokenService, 
            _refreshTokenRepository, 
            _httpContextAccessor);
    }

    // Note: Constructor doesn't validate null parameters since they're injected via DI
    // Null validation happens when methods are actually called

    #region RegisterAsync Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsSuccessWithTokens()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(new List<string> { "User" });

        var expectedAccessToken = "access-token";
        var expectedRefreshToken = "refresh-token";
        var expectedExpiration = DateTime.UtcNow.AddMinutes(60);

        _jwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
            .Returns(expectedAccessToken);
        _jwtTokenService.GenerateRefreshToken().Returns(expectedRefreshToken);
        _jwtTokenService.GetAccessTokenExpiration().Returns(expectedExpiration);

        _refreshTokenRepository.AddAsync(Arg.Any<RefreshToken>())
            .Returns(Task.CompletedTask);

        // Act
        var (response, errors) = await _sut.RegisterAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(errors);
        Assert.Equal(expectedAccessToken, response.AccessToken);
        Assert.Equal(expectedRefreshToken, response.RefreshToken);
        Assert.Equal(request.Email, response.Email);
        Assert.Equal(request.FirstName, response.FirstName);
        Assert.Equal(request.LastName, response.LastName);

        await _userManager.Received(1).CreateAsync(
            Arg.Is<ApplicationUser>(u => 
                u.Email == request.Email && 
                u.UserName == request.Email &&
                u.FirstName == request.FirstName &&
                u.LastName == request.LastName),
            request.Password);
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>());
    }

    [Fact]
    public async Task RegisterAsync_WithFailedUserCreation_ReturnsErrors()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "weak",
            FirstName = "John",
            LastName = "Doe"
        };

        var identityErrors = new[]
        {
            new IdentityError { Description = "Password too short" },
            new IdentityError { Description = "Password requires digit" }
        };

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Failed(identityErrors));

        // Act
        var (response, errors) = await _sut.RegisterAsync(request);

        // Assert
        Assert.Null(response);
        var errorList = errors.ToList();
        Assert.Equal(2, errorList.Count);
        Assert.Contains("Password too short", errorList);
        Assert.Contains("Password requires digit", errorList);

        await _refreshTokenRepository.DidNotReceive().AddAsync(Arg.Any<RefreshToken>());
    }

    [Fact]
    public async Task RegisterAsync_StoresRefreshTokenWithCorrectProperties()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(new List<string>());

        var expectedRefreshToken = "refresh-token";
        var expectedExpiration = DateTime.UtcNow.AddDays(7);

        _jwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
            .Returns("access-token");
        _jwtTokenService.GenerateRefreshToken().Returns(expectedRefreshToken);
        _jwtTokenService.GetRefreshTokenExpiration().Returns(expectedExpiration);

        RefreshToken? capturedToken = null;
        await _refreshTokenRepository.AddAsync(Arg.Do<RefreshToken>(t => capturedToken = t));

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        Assert.NotNull(capturedToken);
        Assert.Equal(expectedRefreshToken, capturedToken.Token);
        Assert.True(capturedToken.ExpiresAt >= DateTime.UtcNow);
        Assert.Equal("127.0.0.1", capturedToken.CreatedByIp);
    }

    #endregion

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessWithTokens()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.Success);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "User" });

        var expectedAccessToken = "access-token";
        var expectedRefreshToken = "refresh-token";
        var expectedExpiration = DateTime.UtcNow.AddMinutes(60);

        _jwtTokenService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>())
            .Returns(expectedAccessToken);
        _jwtTokenService.GenerateRefreshToken().Returns(expectedRefreshToken);
        _jwtTokenService.GetAccessTokenExpiration().Returns(expectedExpiration);

        // Act
        var (response, error) = await _sut.LoginAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Null(error);
        Assert.Equal(expectedAccessToken, response.AccessToken);
        Assert.Equal(expectedRefreshToken, response.RefreshToken);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.Id, response.UserId);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentEmail_ReturnsError()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "nonexistent@example.com",
            Password = "password"
        };

        _userManager.FindByEmailAsync(request.Email).ReturnsNull();

        // Act
        var (response, error) = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(response);
        Assert.Equal("Invalid email or password.", error);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsError()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email
        };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.Failed);

        // Act
        var (response, error) = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(response);
        Assert.Equal("Invalid email or password.", error);
    }

    [Fact]
    public async Task LoginAsync_WithLockedAccount_ReturnsLockedError()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "password"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            UserName = request.Email
        };

        _userManager.FindByEmailAsync(request.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, request.Password, true)
            .Returns(SignInResult.LockedOut);

        // Act
        var (response, error) = await _sut.LoginAsync(request);

        // Assert
        Assert.Null(response);
        Assert.Equal("Account is locked. Please try again later.", error);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var oldToken = "old-refresh-token";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = oldToken,
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = "127.0.0.1"
        };

        _refreshTokenRepository.GetByTokenAsync(oldToken).Returns(storedToken);
        _userManager.GetRolesAsync(user).Returns(new List<string> { "User" });

        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        var expiration = DateTime.UtcNow.AddMinutes(60);

        _jwtTokenService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>>())
            .Returns(newAccessToken);
        _jwtTokenService.GenerateRefreshToken().Returns(newRefreshToken);
        _jwtTokenService.GetAccessTokenExpiration().Returns(expiration);
        _jwtTokenService.GetRefreshTokenExpiration().Returns(DateTime.UtcNow.AddDays(7));

        // Act
        var (response, error) = await _sut.RefreshTokenAsync(oldToken);

        // Assert
        Assert.NotNull(response);
        Assert.Null(error);
        Assert.Equal(newAccessToken, response.AccessToken);
        Assert.Equal(newRefreshToken, response.RefreshToken);
        Assert.Equal(user.Email, response.Email);

        await _refreshTokenRepository.Received(1).UpdateAsync(
            Arg.Is<RefreshToken>(t => 
                t.Token == oldToken && 
                t.RevokedAt != null &&
                t.ReplacedByToken == newRefreshToken));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithNonExistentToken_ReturnsError()
    {
        // Arrange
        var invalidToken = "invalid-token";
        _refreshTokenRepository.GetByTokenAsync(invalidToken).ReturnsNull();

        // Act
        var (response, error) = await _sut.RefreshTokenAsync(invalidToken);

        // Assert
        Assert.Null(response);
        Assert.Equal("Invalid refresh token.", error);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ReturnsError()
    {
        // Arrange
        var expiredToken = "expired-token";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = expiredToken,
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedByIp = "127.0.0.1"
        };

        _refreshTokenRepository.GetByTokenAsync(expiredToken).Returns(storedToken);

        // Act
        var (response, error) = await _sut.RefreshTokenAsync(expiredToken);

        // Assert
        Assert.Null(response);
        Assert.Equal("Refresh token has expired. Please login again.", error);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_RevokesAllUserTokens()
    {
        // Arrange
        var revokedToken = "revoked-token";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = revokedToken,
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = DateTime.UtcNow.AddMinutes(-5), // Already revoked
            RevokedReason = "Manual revocation",
            CreatedByIp = "127.0.0.1"
        };

        _refreshTokenRepository.GetByTokenAsync(revokedToken).Returns(storedToken);

        // Act
        var (response, error) = await _sut.RefreshTokenAsync(revokedToken);

        // Assert
        Assert.Null(response);
        Assert.Contains("Invalid refresh token", error);
        Assert.Contains("All sessions have been terminated", error);

        await _refreshTokenRepository.Received(1).RevokeAllUserTokensAsync(
            user.Id,
            Arg.Is<string>(s => s.Contains("possible token theft")),
            "127.0.0.1");
    }

    [Fact]
    public async Task RefreshTokenAsync_StoresNewRefreshToken()
    {
        // Arrange
        var oldToken = "old-refresh-token";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var storedToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = oldToken,
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            CreatedByIp = "127.0.0.1"
        };

        _refreshTokenRepository.GetByTokenAsync(oldToken).Returns(storedToken);
        _userManager.GetRolesAsync(user).Returns(new List<string>());

        var newRefreshToken = "new-refresh-token";
        _jwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
            .Returns("new-access-token");
        _jwtTokenService.GenerateRefreshToken().Returns(newRefreshToken);
        _jwtTokenService.GetAccessTokenExpiration().Returns(DateTime.UtcNow.AddMinutes(60));
        _jwtTokenService.GetRefreshTokenExpiration().Returns(DateTime.UtcNow.AddDays(7));

        // Act
        await _sut.RefreshTokenAsync(oldToken);

        // Assert
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Is<RefreshToken>(t => 
                t.Token == newRefreshToken && 
                t.UserId == user.Id));
    }

    #endregion

    #region IP Address Tests

    [Fact]
    public async Task RegisterAsync_StoresIpAddressFromHttpContext()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.100");
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(new List<string>());

        _jwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
            .Returns("token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh");
        _jwtTokenService.GetRefreshTokenExpiration().Returns(DateTime.UtcNow.AddDays(7));

        RefreshToken? capturedToken = null;
        await _refreshTokenRepository.AddAsync(Arg.Do<RefreshToken>(t => capturedToken = t));

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        Assert.NotNull(capturedToken);
        Assert.Equal("192.168.1.100", capturedToken.CreatedByIp);
    }

    [Fact]
    public async Task RegisterAsync_WithIPv4MappedToIPv6_StoresIPv4Address()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var httpContext = new DefaultHttpContext();
        // IPv4-mapped IPv6 address: ::ffff:192.168.1.100
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("::ffff:192.168.1.100");
        _httpContextAccessor.HttpContext.Returns(httpContext);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(new List<string>());

        _jwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
            .Returns("token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh");
        _jwtTokenService.GetRefreshTokenExpiration().Returns(DateTime.UtcNow.AddDays(7));

        RefreshToken? capturedToken = null;
        await _refreshTokenRepository.AddAsync(Arg.Do<RefreshToken>(t => capturedToken = t));

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        Assert.NotNull(capturedToken);
        Assert.Equal("192.168.1.100", capturedToken.CreatedByIp);
    }

    [Fact]
    public async Task RegisterAsync_WithNoHttpContext_StoresNullIpAddress()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), Arg.Any<string>())
            .Returns(IdentityResult.Success);
        _userManager.GetRolesAsync(Arg.Any<ApplicationUser>())
            .Returns(new List<string>());

        _jwtTokenService.GenerateAccessToken(Arg.Any<ApplicationUser>(), Arg.Any<IEnumerable<string>>())
            .Returns("token");
        _jwtTokenService.GenerateRefreshToken().Returns("refresh");
        _jwtTokenService.GetRefreshTokenExpiration().Returns(DateTime.UtcNow.AddDays(7));

        RefreshToken? capturedToken = null;
        await _refreshTokenRepository.AddAsync(Arg.Do<RefreshToken>(t => capturedToken = t));

        // Act
        await _sut.RegisterAsync(request);

        // Assert
        Assert.NotNull(capturedToken);
        Assert.Null(capturedToken.CreatedByIp);
    }

    #endregion
}






