using FoodAdviser.Application.DTOs.Receipts;

namespace FoodAdviser.Application.Services.Interfaces;

/// <summary>
/// Service for managing receipt operations including upload, analysis, and retrieval.
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// Uploads and analyzes a receipt image file.
    /// </summary>
    /// <param name="file">The receipt image file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The analyzed and persisted receipt DTO.</returns>
    Task<ReceiptDto> UploadAndAnalyzeReceiptAsync(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent receipts with pagination.
    /// </summary>
    /// <param name="count">Number of recent receipts to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of receipt DTOs.</returns>
    Task<IReadOnlyList<ReceiptDto>> GetRecentReceiptsAsync(int count, CancellationToken cancellationToken = default);
}
