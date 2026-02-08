namespace FoodAdviser.Api.Validation;

/// <summary>
/// Exception thrown when configuration validation fails due to uninjected secret placeholders.
/// </summary>
public class ConfigurationValidationException : Exception
{
    /// <summary>
    /// Gets the configuration section name where validation failed.
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Gets the list of property paths that failed validation.
    /// </summary>
    public IReadOnlyList<string> FailedProperties { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class.
    /// </summary>
    /// <param name="sectionName">The configuration section name where validation failed.</param>
    /// <param name="failedProperties">The list of property paths that failed validation.</param>
    public ConfigurationValidationException(string sectionName, IReadOnlyList<string> failedProperties)
        : base(BuildErrorMessage(sectionName, failedProperties))
    {
        SectionName = sectionName;
        FailedProperties = failedProperties;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class with a custom message.
    /// </summary>
    /// <param name="sectionName">The configuration section name where validation failed.</param>
    /// <param name="failedProperties">The list of property paths that failed validation.</param>
    /// <param name="message">A custom error message.</param>
    public ConfigurationValidationException(string sectionName, IReadOnlyList<string> failedProperties, string message)
        : base(message)
    {
        SectionName = sectionName;
        FailedProperties = failedProperties;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class with an inner exception.
    /// </summary>
    /// <param name="sectionName">The configuration section name where validation failed.</param>
    /// <param name="failedProperties">The list of property paths that failed validation.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    public ConfigurationValidationException(string sectionName, IReadOnlyList<string> failedProperties, Exception innerException)
        : base(BuildErrorMessage(sectionName, failedProperties), innerException)
    {
        SectionName = sectionName;
        FailedProperties = failedProperties;
    }

    /// <summary>
    /// Builds the error message from the section name and failed properties.
    /// </summary>
    private static string BuildErrorMessage(string sectionName, IReadOnlyList<string> failedProperties)
    {
        if (failedProperties.Count == 0)
        {
            return $"Configuration validation failed for section '{sectionName}': No specific errors reported.";
        }

        var errors = failedProperties.Select(prop => $"  - {sectionName}.{prop}: Secret value was not injected");
        return $"Configuration validation failed. The following secrets were not injected from Secrets Manager:\n{string.Join("\n", errors)}";
    }
}
