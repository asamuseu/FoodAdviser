using FoodAdviser.Application.DTOs.Auth;
using FoodAdviser.Application.Services;
using FoodAdviser.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// Implementation of authentication service.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc />
    public async Task<(AuthResponseDto? Response, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDto request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return (null, result.Errors.Select(e => e.Description));
        }

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return (response, Enumerable.Empty<string>());
    }

    /// <inheritdoc />
    public async Task<(AuthResponseDto? Response, string? Error)> LoginAsync(LoginRequestDto request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return (null, "Invalid email or password.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return (null, "Account is locked. Please try again later.");
        }

        if (!result.Succeeded)
        {
            return (null, "Invalid email or password.");
        }

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return (response, null);
    }

    /// <inheritdoc />
    public async Task<(AuthResponseDto? Response, string? Error)> RefreshTokenAsync(string refreshToken)
    {
        // Note: In a production application, you would store refresh tokens in the database
        // and validate them here. This is a simplified implementation.
        // For now, we return an error as refresh token storage is not implemented.
        await Task.CompletedTask;
        return (null, "Refresh token validation not implemented. Please login again.");
    }
}
