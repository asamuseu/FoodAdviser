using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// Factory service that provides the appropriate AI recipe service based on configuration.
/// </summary>
public class AiRecipeServiceFactory : IAiRecipeServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AiProviderOptions _options;
    private readonly ILogger<AiRecipeServiceFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AiRecipeServiceFactory"/> class.
    /// </summary>
    public AiRecipeServiceFactory(
        IServiceProvider serviceProvider,
        IOptions<AiProviderOptions> options,
        ILogger<AiRecipeServiceFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public IAiRecipeService GetService()
    {
        var provider = Enum.TryParse<AiProvider>(_options.ActiveProvider, ignoreCase: true, out var parsed)
            ? parsed
            : AiProvider.OpenAi;

        _logger.LogDebug("Resolving AI recipe service for provider: {Provider}", provider);

        return provider switch
        {
            AiProvider.OpenAi => GetRequiredService<OpenAiService>(),
            _ => GetRequiredService<OpenAiService>()
        };
    }

    /// <inheritdoc />
    public IAiRecipeService GetService(AiProvider provider)
    {
        _logger.LogDebug("Resolving AI recipe service for explicitly requested provider: {Provider}", provider);

        return provider switch
        {
            AiProvider.OpenAi => GetRequiredService<OpenAiService>(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "Unknown AI provider")
        };
    }

    private T GetRequiredService<T>() where T : class
    {
        var service = _serviceProvider.GetService(typeof(T)) as T;
        if (service is null)
        {
            throw new InvalidOperationException($"Service {typeof(T).Name} is not registered.");
        }
        return service;
    }
}
