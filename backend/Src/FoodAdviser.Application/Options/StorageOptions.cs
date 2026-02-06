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

    /// <summary>
    /// Maximum allowed size for uploaded receipt image files, in bytes.
    /// Defaults to 5 MB.
    /// </summary>
    public long MaxReceiptUploadFileSizeBytes { get; set; } = 5L * 1024 * 1024;
}
