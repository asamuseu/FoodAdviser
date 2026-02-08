using System.Reflection;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Api.Validation;

/// <summary>
/// Universal validator for configuration options that detects uninjected secret placeholders.
/// </summary>
public class ConfigurationValidator
{
    private const string SecretPlaceholder = "{injected-from-secrets-manager}";

    /// <summary>
    /// Validates that no properties in the options object contain the secret placeholder value.
    /// </summary>
    /// <typeparam name="TOptions">The type of options to validate.</typeparam>
    /// <param name="options">The options instance to validate.</param>
    /// <param name="sectionName">The configuration section name for error reporting.</param>
    /// <exception cref="ConfigurationValidationException">Thrown when validation fails.</exception>
    public static void ValidateNoPlaceholders<TOptions>(TOptions options, string sectionName) where TOptions : class
    {
        var errors = new List<string>();
        ValidateObject(options, sectionName, string.Empty, errors);

        if (errors.Any())
        {
            throw new ConfigurationValidationException(sectionName, errors);
        }
    }

    /// <summary>
    /// Recursively validates an object and its nested properties for placeholder values.
    /// </summary>
    private static void ValidateObject(object? obj, string sectionName, string propertyPath, List<string> errors)
    {
        if (obj == null)
            return;

        var type = obj.GetType();

        // Skip primitive types, strings (already checked), and collections
        if (type.IsPrimitive || type == typeof(string))
            return;

        // Handle enumerable collections (but not strings)
        if (obj is System.Collections.IEnumerable enumerable and not string)
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    var itemPath = string.IsNullOrEmpty(propertyPath)
                        ? $"[{index}]"
                        : $"{propertyPath}[{index}]";
                    ValidateObject(item, sectionName, itemPath, errors);
                }
                index++;
            }
            return;
        }

        // Check all public properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Skip properties that can't be read
            if (!property.CanRead)
                continue;

            var value = property.GetValue(obj);
            var currentPath = string.IsNullOrEmpty(propertyPath)
                ? property.Name
                : $"{propertyPath}.{property.Name}";

            // Check if the value is the placeholder string
            if (value is string stringValue && stringValue == SecretPlaceholder)
            {
                errors.Add(currentPath);
            }
            // Recursively check nested objects
            else if (value != null && !property.PropertyType.IsPrimitive && property.PropertyType != typeof(string))
            {
                ValidateObject(value, sectionName, currentPath, errors);
            }
        }
    }
}

/// <summary>
/// Options validator that uses <see cref="ConfigurationValidator"/> for validation.
/// Implements IValidateOptions for integration with the Options pattern.
/// </summary>
/// <typeparam name="TOptions">The type of options to validate.</typeparam>
public class SecretPlaceholderValidator<TOptions> : IValidateOptions<TOptions> where TOptions : class
{
    private readonly string _sectionName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretPlaceholderValidator{TOptions}"/> class.
    /// </summary>
    /// <param name="sectionName">The configuration section name for error reporting.</param>
    public SecretPlaceholderValidator(string sectionName)
    {
        _sectionName = sectionName;
    }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        try
        {
            ConfigurationValidator.ValidateNoPlaceholders(options, _sectionName);
            return ValidateOptionsResult.Success;
        }
        catch (ConfigurationValidationException ex)
        {
            return ValidateOptionsResult.Fail(ex.Message);
        }
    }
}
