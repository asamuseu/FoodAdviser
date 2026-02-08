using System.Text;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using FoodAdviser.Infrastructure.Repositories;
using FoodAdviser.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace FoodAdviser.Infrastructure.DependencyInjection;

/// <summary>
/// DI registration helpers for infrastructure.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Infrastructure services and EF Core. The caller must provide the EF Core provider configuration
    /// via <paramref name="configureDb"/> to avoid a hard dependency on a specific provider in this project.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration used for connection strings (kept for future use).</param>
    /// <param name="configureDb">Action to configure the EF Core provider (e.g., options.UseNpgsql(conn)).</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddFoodAdviserInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder> configureDb)
    {
        // Reference configuration to avoid unused parameter warnings; kept for future options mapping.
        _ = configuration;

        if (configureDb is null)
        {
            throw new InvalidOperationException(
                "EF Core provider must be configured. Pass a configuration action, e.g., options => options.UseNpgsql(connectionString).");
        }

        services.AddDbContext<FoodAdviserDbContext>(configureDb);

        // Register HttpContextAccessor for accessing HTTP context in services
        services.AddHttpContextAccessor();

        // Register repositories
        services.AddScoped<IFoodItemRepository, FoodItemRepository>();
        services.AddScoped<IReceiptRepository, ReceiptRepository>();
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Register auth services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }

    /// <summary>
    /// Registers ASP.NET Core Identity with the FoodAdviserDbContext.
    /// Uses ApplicationUser with GUID as primary key.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureIdentity">Optional action to configure Identity options.</param>
    /// <returns>The IdentityBuilder for further configuration.</returns>
    public static IdentityBuilder AddFoodAdviserIdentity(
        this IServiceCollection services,
        Action<IdentityOptions>? configureIdentity = null)
    {
        var builder = services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Default password requirements
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;

            // SignIn settings
            options.SignIn.RequireConfirmedEmail = false;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Apply custom configuration if provided
            configureIdentity?.Invoke(options);
        })
        .AddEntityFrameworkStores<FoodAdviserDbContext>()
        .AddDefaultTokenProviders();

        return builder;
    }

    /// <summary>
    /// Configures JWT Bearer authentication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing JWT settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        services.Configure<JwtOptions>(jwtSection);

        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException("JWT configuration is missing. Please configure the 'Jwt' section in appsettings.");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Validate the issuer
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                // Validate the audience
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                // Validate the token expiry
                ValidateLifetime = true,

                // Validate the signing key
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                // Set clock skew to zero for exact token expiration
                ClockSkew = TimeSpan.Zero
            };

            // Optional: Configure events for debugging or custom handling
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers["Token-Expired"] = "true";
                    }
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorization();

        return services;
    }
}
