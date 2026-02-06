using FoodAdviser.Application.DTOs.Auth;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// Implementation of authentication service.
/// </summary>
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _httpContextAccessor = httpContextAccessor;
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

        // Store refresh token in database
        await StoreRefreshTokenAsync(user.Id, refreshToken);

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

        // Store refresh token in database
        await StoreRefreshTokenAsync(user.Id, refreshToken);

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
        // Retrieve the refresh token from database
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            return (null, "Invalid refresh token.");
        }

        // Check if token is active
        if (!storedToken.IsActive)
        {
            // If token is not active, it might be revoked or expired
            if (storedToken.IsExpired)
            {
                return (null, "Refresh token has expired. Please login again.");
            }

            // Token was revoked - this could indicate token theft, revoke all user tokens
            await _refreshTokenRepository.RevokeAllUserTokensAsync(
                storedToken.UserId,
                "Attempted reuse of revoked token - possible token theft",
                GetIpAddress());

            return (null, "Invalid refresh token. All sessions have been terminated for security reasons.");
        }

        var user = storedToken.User;

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Revoke the old refresh token and create a new one (token rotation)
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedReason = "Replaced by new token";
        storedToken.ReplacedByToken = newRefreshToken;
        storedToken.RevokedByIp = GetIpAddress();
        await _refreshTokenRepository.UpdateAsync(storedToken);

        // Store new refresh token
        await StoreRefreshTokenAsync(user.Id, newRefreshToken);

        var response = new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _jwtTokenService.GetAccessTokenExpiration(),
            UserId = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        return (response, null);
    }

    /// <summary>
    /// Stores a refresh token in the database.
    /// </summary>
    private async Task StoreRefreshTokenAsync(Guid userId, string token)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = _jwtTokenService.GetRefreshTokenExpiration(),
            CreatedByIp = GetIpAddress()
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
    }

    /// <summary>
    /// Gets the IP address from the current HTTP context.
    /// </summary>
    private string? GetIpAddress()
    {
        if (_httpContextAccessor.HttpContext?.Connection.RemoteIpAddress != null)
        {
            var ipAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress;
            
            // Handle IPv4 mapped to IPv6
            if (ipAddress.IsIPv4MappedToIPv6)
            {
                ipAddress = ipAddress.MapToIPv4();
            }
            
            return ipAddress.ToString();
        }

        return null;
    }
}
