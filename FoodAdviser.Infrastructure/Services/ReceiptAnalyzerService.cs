using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Infrastructure.Services;

/// <summary>
/// HttpClient-based receipt analyzer service calling a configurable AI endpoint.
/// </summary>
public class ReceiptAnalyzerService : IReceiptAnalyzerService
{
    private readonly HttpClient _httpClient;
    private readonly ReceiptAnalyzerOptions _options;
    private readonly ILogger<ReceiptAnalyzerService> _logger;

    public ReceiptAnalyzerService(HttpClient httpClient, IOptions<ReceiptAnalyzerOptions> options, ILogger<ReceiptAnalyzerService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }
        _httpClient.Timeout = TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds));
        if (!string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            _httpClient.BaseAddress = new Uri(_options.Endpoint);
        }
    }

    public async Task<Receipt> AnalyzeAsync(string imagePath, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        await using var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var streamContent = new StreamContent(fs);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // best-effort; server may detect
        content.Add(streamContent, name: "file", fileName: Path.GetFileName(imagePath));

        var attempts = Math.Max(1, _options.RetryCount);
        for (var i = 0; i < attempts; i++)
        {
            try
            {
                var response = await _httpClient.PostAsync("/analyze", content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    var parsed = JsonSerializer.Deserialize<AnalyzerResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? throw new InvalidOperationException("Empty analyzer response");
                    return MapToDomain(parsed);
                }
                if ((int)response.StatusCode >= 500 && i < attempts - 1)
                {
                    await Task.Delay(_options.RetryDelayMs, cancellationToken);
                    continue;
                }
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Analyzer returned {(int)response.StatusCode}: {body}");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex) when (i < attempts - 1)
            {
                _logger.LogWarning(ex, "Analyzer attempt {Attempt} failed, retrying...", i + 1);
                await Task.Delay(_options.RetryDelayMs, cancellationToken);
            }
        }
        throw new InvalidOperationException("Analyzer retries exhausted");
    }

    private static Receipt MapToDomain(AnalyzerResponse ar)
    {
        return new Receipt
        {
            Id = Guid.Empty,
            CreatedAt = ar.CreatedAt == default ? DateTimeOffset.UtcNow : ar.CreatedAt,
            Description = ar.Description,
            Items = ar.Items?.Select(i => new ReceiptLineItem
            {
                Id = Guid.NewGuid(),
                Name = i.Name ?? string.Empty,
                Unit = i.Unit ?? "pcs",
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList() ?? new List<ReceiptLineItem>()
        };
    }

    private class AnalyzerResponse
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? Description { get; set; }
        public List<AnalyzerItem>? Items { get; set; }
    }

    private class AnalyzerItem
    {
        public string? Name { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
