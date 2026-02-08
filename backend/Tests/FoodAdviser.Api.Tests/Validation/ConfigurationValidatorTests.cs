using FoodAdviser.Api.Validation;
using FoodAdviser.Application.Options;

namespace FoodAdviser.Api.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ConfigurationValidator"/>.
/// </summary>
public class ConfigurationValidatorTests
{
    private const string SecretPlaceholder = "{injected-from-secrets-manager}";

    [Fact]
    public void ValidateNoPlaceholders_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var options = new OpenAiOptions
        {
            ApiKey = "sk-test-key-12345",
            Model = "gpt-4",
            TimeoutSeconds = 60
        };

        // Act & Assert
        var exception = Record.Exception(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "OpenAi"));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateNoPlaceholders_WithPlaceholderInApiKey_ShouldThrowConfigurationValidationException()
    {
        // Arrange
        var options = new OpenAiOptions
        {
            ApiKey = SecretPlaceholder,
            Model = "gpt-4",
            TimeoutSeconds = 60
        };

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "OpenAi"));

        Assert.Equal("OpenAi", exception.SectionName);
        Assert.Contains("ApiKey", exception.FailedProperties);
        Assert.Contains("OpenAi.ApiKey", exception.Message);
        Assert.Contains("Secret value was not injected", exception.Message);
    }

    [Fact]
    public void ValidateNoPlaceholders_WithMultiplePlaceholders_ShouldReportAllErrors()
    {
        // Arrange
        var options = new ReceiptAnalyzerOptions
        {
            ClientId = SecretPlaceholder,
            Username = "valid-username",
            ApiKey = SecretPlaceholder,
            TimeoutSeconds = 30,
            RetryCount = 3,
            RetryDelayMs = 500
        };

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "ReceiptAnalyzer"));

        Assert.Equal("ReceiptAnalyzer", exception.SectionName);
        Assert.Equal(2, exception.FailedProperties.Count);
        Assert.Contains("ClientId", exception.FailedProperties);
        Assert.Contains("ApiKey", exception.FailedProperties);
        Assert.Contains("ReceiptAnalyzer.ClientId", exception.Message);
        Assert.Contains("ReceiptAnalyzer.ApiKey", exception.Message);
    }

    [Fact]
    public void ValidateNoPlaceholders_WithStorageOptions_ShouldDetectPlaceholder()
    {
        // Arrange
        var options = new StorageOptions
        {
            ReceiptTempPath = SecretPlaceholder,
            MaxReceiptUploadFileSizeBytes = 5242880
        };

        // Act & Assert
        var exception = Assert.Throws<ConfigurationValidationException>(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "Storage"));

        Assert.Equal("Storage", exception.SectionName);
        Assert.Contains("ReceiptTempPath", exception.FailedProperties);
        Assert.Contains("Storage.ReceiptTempPath", exception.Message);
    }

    [Fact]
    public void ValidateNoPlaceholders_WithNullValue_ShouldNotThrow()
    {
        // Arrange
        var options = new StorageOptions
        {
            ReceiptTempPath = null,
            MaxReceiptUploadFileSizeBytes = 5242880
        };

        // Act & Assert
        var exception = Record.Exception(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "Storage"));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateNoPlaceholders_WithEmptyString_ShouldNotThrow()
    {
        // Arrange
        var options = new OpenAiOptions
        {
            ApiKey = string.Empty,
            Model = "gpt-4",
            TimeoutSeconds = 60
        };

        // Act & Assert
        var exception = Record.Exception(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "OpenAi"));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateNoPlaceholders_WithComplexObject_AllValidValues_ShouldNotThrow()
    {
        // Arrange
        var options = new ReceiptAnalyzerOptions
        {
            ClientId = "client-123",
            Username = "user@example.com",
            ApiKey = "api-key-xyz",
            TimeoutSeconds = 180,
            RetryCount = 3,
            RetryDelayMs = 500
        };

        // Act & Assert
        var exception = Record.Exception(() =>
            ConfigurationValidator.ValidateNoPlaceholders(options, "ReceiptAnalyzer"));

        Assert.Null(exception);
    }
}

/// <summary>
/// Unit tests for <see cref="SecretPlaceholderValidator{TOptions}"/>.
/// </summary>
public class SecretPlaceholderValidatorTests
{
    private const string SecretPlaceholder = "{injected-from-secrets-manager}";

    [Fact]
    public void Validate_WithValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var validator = new SecretPlaceholderValidator<OpenAiOptions>("OpenAi");
        var options = new OpenAiOptions
        {
            ApiKey = "sk-test-key-12345",
            Model = "gpt-4",
            TimeoutSeconds = 60
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_WithPlaceholder_ReturnsFailure()
    {
        // Arrange
        var validator = new SecretPlaceholderValidator<OpenAiOptions>("OpenAi");
        var options = new OpenAiOptions
        {
            ApiKey = SecretPlaceholder,
            Model = "gpt-4",
            TimeoutSeconds = 60
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("OpenAi.ApiKey", result.FailureMessage);
    }

    [Fact]
    public void Validate_WithMultiplePlaceholders_ReturnsFailureWithAllErrors()
    {
        // Arrange
        var validator = new SecretPlaceholderValidator<ReceiptAnalyzerOptions>("ReceiptAnalyzer");
        var options = new ReceiptAnalyzerOptions
        {
            ClientId = SecretPlaceholder,
            Username = SecretPlaceholder,
            ApiKey = SecretPlaceholder,
            TimeoutSeconds = 30
        };

        // Act
        var result = validator.Validate(null, options);

        // Assert
        Assert.True(result.Failed);
        Assert.Contains("ReceiptAnalyzer.ClientId", result.FailureMessage);
        Assert.Contains("ReceiptAnalyzer.Username", result.FailureMessage);
        Assert.Contains("ReceiptAnalyzer.ApiKey", result.FailureMessage);
    }
}
