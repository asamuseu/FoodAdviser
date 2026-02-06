namespace FoodAdviser.Application.Options;

/// <summary>
/// Configuration options for OpenAI integration.
/// </summary>
public class OpenAiOptions
{
    /// <summary>
    /// The OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The model to use for recipe generation (e.g., "gpt-4", "gpt-3.5-turbo").
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 60;
}
