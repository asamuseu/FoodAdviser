namespace FoodAdviser.Application.Options;

/// <summary>
/// Configuration options for AI provider selection.
/// </summary>
public class AiProviderOptions
{
    /// <summary>
    /// The active AI provider to use for recipe generation.
    /// Valid values: "OpenAi", "DeepSeek"
    /// </summary>
    public string ActiveProvider { get; set; } = "OpenAi";
}

/// <summary>
/// Enum representing available AI providers.
/// </summary>
public enum AiProvider
{
    /// <summary>
    /// OpenAI provider (GPT models).
    /// </summary>
    OpenAi,

    /// <summary>
    /// DeepSeek provider.
    /// </summary>
    DeepSeek
}
