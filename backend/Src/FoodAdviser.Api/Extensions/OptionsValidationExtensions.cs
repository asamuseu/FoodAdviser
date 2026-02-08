using FoodAdviser.Api.Validation;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Api.Extensions;

/// <summary>
/// Extension methods for registering and validating configuration options.
/// </summary>
public static class OptionsValidationExtensions
{
    private static readonly List<Type> RegisteredOptionsTypes = new();

    /// <summary>
    /// Configures an options instance and registers validation for secret placeholders.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The name of the configuration section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureAndValidate<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName) where TOptions : class
    {
        services.Configure<TOptions>(configuration.GetSection(sectionName));
        services.AddSingleton<IValidateOptions<TOptions>>(
            new SecretPlaceholderValidator<TOptions>(sectionName));

        // Track this options type for later validation
        lock (RegisteredOptionsTypes)
        {
            if (!RegisteredOptionsTypes.Contains(typeof(TOptions)))
            {
                RegisteredOptionsTypes.Add(typeof(TOptions));
            }
        }

        return services;
    }

    /// <summary>
    /// Validates all registered options by forcing their resolution.
    /// This should be called during application startup to ensure all configurations are valid.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="OptionsValidationException">Thrown when any options validation fails.</exception>
    public static void ValidateAllOptions(this IServiceProvider serviceProvider)
    {
        List<Type> optionsTypesToValidate;
        lock (RegisteredOptionsTypes)
        {
            optionsTypesToValidate = new List<Type>(RegisteredOptionsTypes);
        }

        if (!optionsTypesToValidate.Any())
        {
            // No validators registered, nothing to validate
            return;
        }

        var errors = new List<string>();

        // Force validation by resolving IOptionsMonitor for each registered options type
        foreach (var optionsType in optionsTypesToValidate)
        {
            var monitorType = typeof(IOptionsMonitor<>).MakeGenericType(optionsType);

            try
            {
                // Resolving IOptionsMonitor<T> will trigger validation
                var monitor = serviceProvider.GetRequiredService(monitorType);

                // Access CurrentValue to force validation
                var currentValueProperty = monitorType.GetProperty("CurrentValue");
                if (currentValueProperty != null)
                {
                    _ = currentValueProperty.GetValue(monitor);
                }
            }
            catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is OptionsValidationException)
            {
                // Unwrap the TargetInvocationException to get the actual OptionsValidationException
                var optionsEx = (OptionsValidationException)ex.InnerException;
                errors.Add($"Configuration validation failed for {optionsType.Name}:\n{optionsEx.Message}");
            }
            catch (OptionsValidationException ex)
            {
                // Collect error but continue checking other options
                errors.Add($"Configuration validation failed for {optionsType.Name}:\n{ex.Message}");
            }
        }

        // If any errors occurred, throw with all error details
        if (errors.Any())
        {
            throw new InvalidOperationException(
                $"One or more configuration sections failed validation:\n\n{string.Join("\n\n", errors)}");
        }
    }
}
