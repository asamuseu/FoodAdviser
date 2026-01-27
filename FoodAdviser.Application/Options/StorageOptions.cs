namespace FoodAdviser.Application.Options;

/// <summary>
/// Options for storage-related paths.
/// </summary>
public class StorageOptions
{
    /// <summary>
    /// Temporary path for uploaded receipt images.
    /// </summary>
    public string? ReceiptTempPath { get; set; }
}
