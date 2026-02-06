using Microsoft.AspNetCore.Http;

namespace FoodAdviser.Api.DTOs.Receipts;

/// <summary>
/// Request payload for uploading a receipt image via multipart/form-data.
/// </summary>
public sealed class UploadReceiptDto
{
    /// <summary>
    /// The image file to upload (PNG or JPEG).
    /// </summary>
    public IFormFile? File { get; set; }
}
