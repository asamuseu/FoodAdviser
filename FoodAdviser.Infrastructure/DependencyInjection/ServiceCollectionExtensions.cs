using FoodAdviser.Domain.Repositories;
using FoodAdviser.Infrastructure.Persistence;
using FoodAdviser.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IFoodItemRepository, FoodItemRepository>();
        // ... add other repositories when implemented
        return services;
    }
}
