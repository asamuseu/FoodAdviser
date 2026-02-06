using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Application.Services.Interfaces;

/// <summary>
/// Service that analyzes receipt images and extracts structured data.
/// </summary>
public interface IReceiptAnalyzerService
{
    /// <summary>
    /// Analyze the receipt image at the given file path and return a parsed <see cref="Receipt"/>.
    /// </summary>
    /// <param name="imagePath">Path to the temporary image file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed receipt.</returns>
    Task<Receipt> AnalyzeAsync(string imagePath, CancellationToken cancellationToken = default);
}
