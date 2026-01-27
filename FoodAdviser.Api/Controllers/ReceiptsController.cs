using AutoMapper;
using FoodAdviser.Application.DTOs.Receipts;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Api.Controllers;

/// <summary>
/// Handles receipt parsing and retrieval.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptAnalyzerService _analyzer;
    private readonly IReceiptRepository _repository;
    private readonly IMapper _mapper;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(
        IReceiptAnalyzerService analyzer,
        IReceiptRepository repository,
        IMapper mapper,
        IOptions<StorageOptions> storageOptions,
        ILogger<ReceiptsController> logger)
    {
        _analyzer = analyzer;
        _repository = repository;
        _mapper = mapper;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    /// <summary>Analyzes a raw receipt payload and returns a summary.</summary>
    /// <remarks>Stub endpoint for future implementation.</remarks>
    [HttpPost("analyze")]
    public IActionResult Analyze([FromBody] object payload)
    {
        return Accepted(new { message = "Receipt analysis queued" });
    }

    /// <summary>Gets recent receipts.</summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent(CancellationToken cancellationToken)
    {
        var receipts = await _repository.GetRecentAsync(10, cancellationToken);
        return Ok(receipts.Select(r => _mapper.Map<ReceiptDto>(r)).ToArray());
    }

    /// <summary>
    /// Uploads a receipt image, analyzes it via AI, persists the parsed receipt and returns it.
    /// </summary>
    /// <param name="file">The image file to upload (PNG or JPEG).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReceiptDto>> UploadReceipt([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(file)] = new[] { "File is required." }
            }) { Title = "Invalid upload" });
        }
        if (file.Length == 0)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(file)] = new[] { "File is empty." }
            }) { Title = "Invalid upload" });
        }
        var allowed = new[] { "image/png", "image/jpeg" };
        if (!allowed.Contains(file.ContentType))
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                [nameof(file)] = new[] { "Only PNG or JPEG images are allowed." }
            }) { Title = "Invalid upload" });
        }

        var baseTemp = _storageOptions.ReceiptTempPath ?? Path.Combine(Path.GetTempPath(), "FoodAdviser", "receipts");
        Directory.CreateDirectory(baseTemp);
        var ext = Path.GetExtension(file.FileName);
        var tempFile = Path.Combine(baseTemp, $"{Guid.NewGuid()}{ext}");

        try
        {
            await using (var fs = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(fs, cancellationToken);
            }

            var receipt = await _analyzer.AnalyzeAsync(tempFile, cancellationToken);
            var added = await _repository.AddAsync(receipt, cancellationToken);
            var dto = _mapper.Map<ReceiptDto>(added);
            return CreatedAtAction(nameof(GetRecent), new { }, dto);
        }
        catch (OperationCanceledException)
        {
            return Problem(title: "Upload canceled", statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze uploaded receipt");
            return Problem(title: "Failed to analyze receipt", statusCode: StatusCodes.Status500InternalServerError);
        }
        finally
        {
            try { System.IO.File.Delete(tempFile); } catch { /* ignore */ }
        }
    }
}
