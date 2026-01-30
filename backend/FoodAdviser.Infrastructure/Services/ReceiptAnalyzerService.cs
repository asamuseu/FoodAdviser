using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Veryfi;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// Veryfi SDK-based receipt analyzer service.
/// </summary>
public class ReceiptAnalyzerService : IReceiptAnalyzerService
{
    private readonly ReceiptAnalyzerOptions _options;
    private readonly ILogger<ReceiptAnalyzerService> _logger;

    public ReceiptAnalyzerService(IOptions<ReceiptAnalyzerOptions> options, ILogger<ReceiptAnalyzerService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Receipt> AnalyzeAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        var attempts = Math.Max(1, _options.RetryCount);
        for (var i = 0; i < attempts; i++)
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds))
                };
                var api = new VeryfiApi(_options.Username, _options.ApiKey, _options.ClientId, httpClient);
                var bytes = await File.ReadAllBytesAsync(imagePath, cancellationToken);
                var request = new DocumentPOSTJSONRequest
                {
                    File_name = Path.GetFileName(imagePath),
                    File_data = Convert.ToBase64String(bytes)
                };
                var documentResponse = await api.Documents2Async(request, cancellationToken: cancellationToken);
                
                var document = await api.ProcessDocumentAsync(
                    new DocumentUploadOptionsV7() {
                        File_name = "fileName.jpg",
                        File_data = Convert.ToBase64String(bytes),
                    }, cancellationToken);
                
                return MapToDomain(documentResponse);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (i < attempts - 1)
            {
                _logger.LogWarning(ex, "Veryfi analyzer attempt {Attempt} failed, retrying...", i + 1);
                await Task.Delay(_options.RetryDelayMs, cancellationToken);
            }
        }
        throw new InvalidOperationException("Analyzer retries exhausted");
    }

    private static Receipt MapToDomain(object documentResponse)
    {
        Document document = documentResponse as Document 
            ?? throw new InvalidCastException("Failed to cast Veryfi response to Document");
        
        var items = new List<ReceiptLineItem>();
        if (document.Line_items != null)
        {
            foreach (var li in document.Line_items)
            {
                items.Add(new ReceiptLineItem
                {
                    Id = Guid.NewGuid(),
                    Name = li.Description ?? li.Text ?? string.Empty,
                    Unit = li.Unit_of_measure ?? "pcs",
                    Quantity = (decimal)li.Quantity
                });
            }
        }
        return new Receipt
        {
            Id = Guid.Empty,
            CreatedAt = DateTimeOffset.UtcNow,
            Items = items
        };
    }
}
