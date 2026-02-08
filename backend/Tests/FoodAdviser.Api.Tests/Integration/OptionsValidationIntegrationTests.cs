using FoodAdviser.Api.Extensions;
using FoodAdviser.Application.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Api.Tests.Integration;

/// <summary>
/// Integration tests for configuration validation during application startup.
/// </summary>
public class OptionsValidationIntegrationTests
{
    [Fact]
    public void ValidateAllOptions_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAi:ApiKey"] = "sk-test-key-12345",
                ["OpenAi:Model"] = "gpt-4",
                ["OpenAi:TimeoutSeconds"] = "60",
                ["Storage:ReceiptTempPath"] = "C:\\temp\\receipts",
                ["Storage:MaxReceiptUploadFileSizeBytes"] = "5242880"
            })
            .Build();

        var services = new ServiceCollection();
        services.ConfigureAndValidate<OpenAiOptions>(configuration, "OpenAi");
        services.ConfigureAndValidate<StorageOptions>(configuration, "Storage");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Record.Exception(() => serviceProvider.ValidateAllOptions());
        Assert.Null(exception);
    }

    [Fact]
    public void ValidateAllOptions_WithPlaceholder_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAi:ApiKey"] = "{injected-from-secrets-manager}",
                ["OpenAi:Model"] = "gpt-4",
                ["OpenAi:TimeoutSeconds"] = "60"
            })
            .Build();

        var services = new ServiceCollection();
        services.ConfigureAndValidate<OpenAiOptions>(configuration, "OpenAi");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.ValidateAllOptions());

        Assert.Contains("Configuration validation failed", exception.Message);
        Assert.Contains("OpenAiOptions", exception.Message);
    }

    [Fact]
    public void ValidateAllOptions_WithMultipleSectionsAndPlaceholders_ShouldThrow()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAi:ApiKey"] = "{injected-from-secrets-manager}",
                ["OpenAi:Model"] = "gpt-4",
                ["OpenAi:TimeoutSeconds"] = "60",
                ["Storage:ReceiptTempPath"] = "C:\\temp\\receipts",
                ["Storage:MaxReceiptUploadFileSizeBytes"] = "5242880"
            })
            .Build();

        var services = new ServiceCollection();
        services.ConfigureAndValidate<OpenAiOptions>(configuration, "OpenAi");
        services.ConfigureAndValidate<StorageOptions>(configuration, "Storage");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.ValidateAllOptions());

        Assert.Contains("OpenAi.ApiKey", exception.Message);
    }

    [Fact]
    public void ConfigureAndValidate_RegistersValidatorCorrectly()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAi:ApiKey"] = "sk-test-key-12345",
                ["OpenAi:Model"] = "gpt-4",
                ["OpenAi:TimeoutSeconds"] = "60"
            })
            .Build();

        var services = new ServiceCollection();
        services.ConfigureAndValidate<OpenAiOptions>(configuration, "OpenAi");

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>();
        var validator = serviceProvider.GetServices<IValidateOptions<OpenAiOptions>>().FirstOrDefault();

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(validator);
        Assert.Equal("sk-test-key-12345", options.Value.ApiKey);
    }

    [Fact]
    public void IOptionsMonitor_WithPlaceholder_ThrowsOnAccess()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ReceiptAnalyzer:ClientId"] = "{injected-from-secrets-manager}",
                ["ReceiptAnalyzer:Username"] = "user@example.com",
                ["ReceiptAnalyzer:ApiKey"] = "valid-key",
                ["ReceiptAnalyzer:TimeoutSeconds"] = "180",
                ["ReceiptAnalyzer:RetryCount"] = "3",
                ["ReceiptAnalyzer:RetryDelayMs"] = "500"
            })
            .Build();

        var services = new ServiceCollection();
        services.ConfigureAndValidate<ReceiptAnalyzerOptions>(configuration, "ReceiptAnalyzer");

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<ReceiptAnalyzerOptions>>();
        var exception = Assert.Throws<OptionsValidationException>(() => optionsMonitor.CurrentValue);

        Assert.Contains("ReceiptAnalyzer.ClientId", exception.Message);
    }
}
