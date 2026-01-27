namespace FoodAdviser.Application.Options;

/// <summary>
/// Configuration options for the receipt analyzer AI endpoint.
/// </summary>
public class ReceiptAnalyzerOptions
{
    /// <summary>Base URL of the analyzer API.</summary>
    public string Endpoint { get; set; } = string.Empty;
    /// <summary>API key or bearer token for the analyzer API.</summary>
    public string? ApiKey { get; set; }
    /// <summary>Request timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;
    /// <summary>Number of retries for transient failures.</summary>
    public int RetryCount { get; set; } = 3;
    /// <summary>Delay between retries in milliseconds.</summary>
    public int RetryDelayMs { get; set; } = 500;
}
