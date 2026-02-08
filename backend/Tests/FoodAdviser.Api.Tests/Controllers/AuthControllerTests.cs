using FoodAdviser.Api.Controllers;
using FoodAdviser.Api.DTOs.Auth;
using FoodAdviser.Application.DTOs.Auth;
using FoodAdviser.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace FoodAdviser.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly IAuthService _authService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authService = Substitute.For<IAuthService>();
        _controller = new AuthController(_authService);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var expectedResponse = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            Email = request.Email,
            UserId = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authService.RegisterAsync(request)
            .Returns((expectedResponse, Enumerable.Empty<string>()));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
        Assert.Equal(expectedResponse.Email, response.Email);
    }

    [Fact]
    public async Task Register_WithFailedRegistration_ReturnsValidationProblem()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "weak",
            ConfirmPassword = "weak",
            FirstName = "John",
            LastName = "Doe"
        };

        var errors = new[] { "Password too short", "Password requires digit" };
        _authService.RegisterAsync(request)
            .Returns(((AuthResponseDto?)null, errors));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var validationResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationResult.StatusCode);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(validationResult.Value);
        Assert.NotEmpty(problemDetails.Errors);
    }

    [Fact]
    public async Task Register_WithEmptyErrors_ReturnsValidationProblem()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            FirstName = "John",
            LastName = "Doe"
        };

        _authService.RegisterAsync(request)
            .Returns(((AuthResponseDto?)null, Enumerable.Empty<string>()));

        // Act
        var result = await _controller.Register(request);

        // Assert
        var validationResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationResult.StatusCode);
        var problemDetails = Assert.IsType<ValidationProblemDetails>(validationResult.Value);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        var expectedResponse = new AuthResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            Email = request.Email,
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authService.LoginAsync(request)
            .Returns((expectedResponse, (string?)null));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
        Assert.Equal(expectedResponse.Email, response.Email);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var errorMessage = "Invalid email or password.";
        _authService.LoginAsync(request)
            .Returns(((AuthResponseDto?)null, errorMessage));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Equal("Authentication failed", problemDetails.Title);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
    }

    [Fact]
    public async Task Login_WithLockedAccount_ReturnsUnauthorizedWithLockedMessage()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        var errorMessage = "Account is locked. Please try again later.";
        _authService.LoginAsync(request)
            .Returns(((AuthResponseDto?)null, errorMessage));

        // Act
        var result = await _controller.Login(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Contains("locked", problemDetails.Detail!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var expectedResponse = new AuthResponseDto
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            Email = "test@example.com",
            UserId = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _authService.RefreshTokenAsync(request.RefreshToken)
            .Returns((expectedResponse, (string?)null));

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
        Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid_token"
        };

        var errorMessage = "Invalid refresh token.";
        _authService.RefreshTokenAsync(request.RefreshToken)
            .Returns(((AuthResponseDto?)null, errorMessage));

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Equal("Token refresh failed", problemDetails.Title);
        Assert.Equal(errorMessage, problemDetails.Detail);
        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ReturnsUnauthorized()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "expired_token"
        };

        var errorMessage = "Refresh token has expired. Please login again.";
        _authService.RefreshTokenAsync(request.RefreshToken)
            .Returns(((AuthResponseDto?)null, errorMessage));

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Contains("expired", problemDetails.Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_ReturnsUnauthorizedWithSecurityMessage()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "revoked_token"
        };

        var errorMessage = "Invalid refresh token. All sessions have been terminated for security reasons.";
        _authService.RefreshTokenAsync(request.RefreshToken)
            .Returns(((AuthResponseDto?)null, errorMessage));

        // Act
        var result = await _controller.RefreshToken(request);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(unauthorizedResult.Value);
        Assert.Contains("security", problemDetails.Detail!, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}





