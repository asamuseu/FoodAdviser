namespace FoodAdviser.Application.Options;

/// <summary>
/// Configuration options for the Veryfi receipt analyzer SDK.
/// </summary>
public class ReceiptAnalyzerOptions
{
    /// <summary>Veryfi Client ID.</summary>
    public string ClientId { get; set; } = string.Empty;
    /// <summary>Veryfi Username.</summary>
    public string Username { get; set; } = string.Empty;
    /// <summary>Veryfi API Key.</summary>
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
    /// <summary>Number of retries for transient failures.</summary>
    public int RetryCount { get; set; } = 3;
    /// <summary>Delay between retries in milliseconds.</summary>
    public int RetryDelayMs { get; set; } = 500;
}
