using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

                // var document = await api.ProcessDocumentAsync(
                //     new DocumentUploadOptionsV7() {
                //         File_name = "fileName.jpg",
                //         File_data = Convert.ToBase64String(bytes),
                //     }, cancellationToken);

                var documentResponse = await api.Documents2Async(request, cancellationToken: cancellationToken);

                var json = documentResponse.ToString();
                var document = JsonConvert.DeserializeObject<Document>(json, JsonSettings) ?? new Document();

                return MapToDomain(document);
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

    private static Receipt MapToDomain(Document document)
    {
        if (document == null)
        {
            return null;
        }

        var items = new List<ReceiptLineItem>();
        if (document.Line_items != null)
        {
            foreach (var li in document.Line_items)
            {
                if (li.Type != LineItemType.Food)
                {
                    continue;
                }

                items.Add(new ReceiptLineItem
                {
                    Id = Guid.NewGuid(),
                    Name = li.Description ?? li.Text ?? string.Empty,
                    Unit = li.Unit_of_measure ?? "pcs",
                    Quantity = (decimal)li.Quantity,
                    Price = (decimal)li.Price == 0 ? (decimal)li.Total : (decimal)li.Price
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

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,

        // Optional: ignore individual value conversion errors instead of throwing.
        Error = (sender, args) =>
        {
            args.ErrorContext.Handled = true;
        }
    };
}
