using FoodAdviser.Application.DTOs.Receipts;
using FoodAdviser.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodAdviser.Api.DTOs.Receipts;

namespace FoodAdviser.Api.Controllers;

/// <summary>
/// Handles receipt parsing and retrieval.
/// All operations are scoped to the authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly IReceiptService _receiptService;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(
        IReceiptService receiptService,
        ILogger<ReceiptsController> logger)
    {
        _receiptService = receiptService;
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
        var receipts = await _receiptService.GetRecentReceiptsAsync(10, cancellationToken);
        return Ok(receipts);
    }

    /// <summary>
    /// Uploads a receipt image, analyzes it via AI, persists the parsed receipt and returns it.
    /// </summary>
    /// <param name="request">The upload request containing the image file (PNG or JPEG).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReceiptDto>> UploadReceipt([FromForm] UploadReceiptDto request, CancellationToken cancellationToken)
    {
        try
        {
            var file = request.File!;
            var dto = await _receiptService.UploadAndAnalyzeReceiptAsync(file, cancellationToken);
            return CreatedAtAction(nameof(GetRecent), new { }, dto);
        }
        catch (OperationCanceledException)
        {
            return Problem(title: "Upload canceled", statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process receipt upload");
            return Problem(title: "Failed to analyze receipt", statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
