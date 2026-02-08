using FoodAdviser.Api.Validation;

namespace FoodAdviser.Api.Tests.Validation;

/// <summary>
/// Unit tests for <see cref="ConfigurationValidationException"/>.
/// </summary>
public class ConfigurationValidationExceptionTests
{
    [Fact]
    public void Constructor_WithSectionAndProperties_SetsPropertiesCorrectly()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey", "Model" };

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties);

        // Assert
        Assert.Equal(sectionName, exception.SectionName);
        Assert.Equal(failedProperties.Count, exception.FailedProperties.Count);
        Assert.Contains("ApiKey", exception.FailedProperties);
        Assert.Contains("Model", exception.FailedProperties);
    }

    [Fact]
    public void Constructor_WithSingleProperty_BuildsCorrectMessage()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey" };

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties);

        // Assert
        Assert.Contains("Configuration validation failed", exception.Message);
        Assert.Contains("OpenAi.ApiKey", exception.Message);
        Assert.Contains("Secret value was not injected", exception.Message);
    }

    [Fact]
    public void Constructor_WithMultipleProperties_BuildsCorrectMessage()
    {
        // Arrange
        var sectionName = "ReceiptAnalyzer";
        var failedProperties = new List<string> { "ClientId", "ApiKey", "Username" };

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties);

        // Assert
        Assert.Contains("Configuration validation failed", exception.Message);
        Assert.Contains("ReceiptAnalyzer.ClientId", exception.Message);
        Assert.Contains("ReceiptAnalyzer.ApiKey", exception.Message);
        Assert.Contains("ReceiptAnalyzer.Username", exception.Message);
        Assert.Contains("Secret value was not injected", exception.Message);
    }

    [Fact]
    public void Constructor_WithEmptyProperties_BuildsNoErrorsMessage()
    {
        // Arrange
        var sectionName = "TestSection";
        var failedProperties = Array.Empty<string>();

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties);

        // Assert
        Assert.Contains("Configuration validation failed for section 'TestSection'", exception.Message);
        Assert.Contains("No specific errors reported", exception.Message);
    }

    [Fact]
    public void Constructor_WithCustomMessage_UsesProvidedMessage()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey" };
        var customMessage = "Custom error message";

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties, customMessage);

        // Assert
        Assert.Equal(customMessage, exception.Message);
        Assert.Equal(sectionName, exception.SectionName);
        Assert.Contains("ApiKey", exception.FailedProperties);
    }

    [Fact]
    public void Constructor_WithInnerException_PreservesInnerException()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey" };
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties, innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
        Assert.Equal(sectionName, exception.SectionName);
        Assert.Contains("ApiKey", exception.FailedProperties);
    }

    [Fact]
    public void FailedProperties_IsReadOnly()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey" };

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties);

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<string>>(exception.FailedProperties);
    }

    [Fact]
    public void Exception_CanBeCaughtAsException()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey" };

        // Act & Assert
        var caught = false;
        try
        {
            throw new ConfigurationValidationException(sectionName, failedProperties);
        }
        catch (Exception ex)
        {
            caught = true;
            Assert.IsType<ConfigurationValidationException>(ex);
        }

        Assert.True(caught, "Exception should have been caught");
    }

    [Fact]
    public void Exception_CanBeCaughtAsConfigurationValidationException()
    {
        // Arrange
        var sectionName = "OpenAi";
        var failedProperties = new List<string> { "ApiKey" };

        // Act & Assert
        var caught = false;
        try
        {
            throw new ConfigurationValidationException(sectionName, failedProperties);
        }
        catch (ConfigurationValidationException ex)
        {
            caught = true;
            Assert.Equal("OpenAi", ex.SectionName);
        }

        Assert.True(caught, "Exception should have been caught");
    }

    [Fact]
    public void MessageFormat_MatchesExpectedStructure()
    {
        // Arrange
        var sectionName = "Storage";
        var failedProperties = new List<string> { "ReceiptTempPath" };

        // Act
        var exception = new ConfigurationValidationException(sectionName, failedProperties);

        // Assert
        var lines = exception.Message.Split('\n');
        Assert.True(lines.Length >= 2, "Message should have at least 2 lines");
        Assert.Contains("Configuration validation failed", lines[0]);
        Assert.Contains("Storage.ReceiptTempPath", exception.Message);
    }
}
