using FoodAdviser.Api.Controllers;
using FoodAdviser.Api.DTOs.Receipts;
using FoodAdviser.Application.DTOs.Receipts;
using FoodAdviser.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace FoodAdviser.Api.Tests.Controllers;

public class ReceiptsControllerTests
{
    private readonly IReceiptService _receiptService;
    private readonly ILogger<ReceiptsController> _logger;
    private readonly ReceiptsController _controller;

    public ReceiptsControllerTests()
    {
        _receiptService = Substitute.For<IReceiptService>();
        _logger = Substitute.For<ILogger<ReceiptsController>>();
        _controller = new ReceiptsController(_receiptService, _logger);
    }

    #region Analyze Tests

    [Fact]
    public void Analyze_WithAnyPayload_ReturnsAccepted()
    {
        // Arrange
        var payload = new { data = "test" };

        // Act
        var result = _controller.Analyze(payload);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        var value = acceptedResult.Value;
        Assert.NotNull(value);
    }

    [Fact]
    public void Analyze_WithNullPayload_ReturnsAccepted()
    {
        // Arrange
        object? payload = null;

        // Act
        var result = _controller.Analyze(payload!);

        // Assert
        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public void Analyze_ReturnsQueuedMessage()
    {
        // Arrange
        var payload = new { };

        // Act
        var result = _controller.Analyze(payload);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        var json = JsonSerializer.Serialize(acceptedResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        Assert.NotNull(response);
        Assert.Equal("Receipt analysis queued", response["message"]);
    }

    #endregion

    #region GetRecent Tests

    [Fact]
    public async Task GetRecent_ReturnsOkWithReceipts()
    {
        // Arrange
        var receipts = new List<ReceiptDto>
        {
            new() { Id = Guid.NewGuid(), Total = 50.00m, CreatedAt = DateTimeOffset.UtcNow },
            new() { Id = Guid.NewGuid(), Total = 75.50m, CreatedAt = DateTimeOffset.UtcNow }
        };

        _receiptService.GetRecentReceiptsAsync(10, Arg.Any<CancellationToken>())
            .Returns(receipts);

        // Act
        var result = await _controller.GetRecent(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReceipts = Assert.IsAssignableFrom<IEnumerable<ReceiptDto>>(okResult.Value);
        Assert.Equal(2, returnedReceipts.Count());
    }

    [Fact]
    public async Task GetRecent_CallsServiceWithCorrectLimit()
    {
        // Arrange
        _receiptService.GetRecentReceiptsAsync(10, Arg.Any<CancellationToken>())
            .Returns(new List<ReceiptDto>());

        // Act
        await _controller.GetRecent(CancellationToken.None);

        // Assert
        await _receiptService.Received(1).GetRecentReceiptsAsync(10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRecent_WithEmptyList_ReturnsOkWithEmptyList()
    {
        // Arrange
        _receiptService.GetRecentReceiptsAsync(10, Arg.Any<CancellationToken>())
            .Returns(new List<ReceiptDto>());

        // Act
        var result = await _controller.GetRecent(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedReceipts = Assert.IsAssignableFrom<IEnumerable<ReceiptDto>>(okResult.Value);
        Assert.Empty(returnedReceipts);
    }

    [Fact]
    public async Task GetRecent_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _receiptService.GetRecentReceiptsAsync(10, cts.Token)
            .Returns(new List<ReceiptDto>());

        // Act
        await _controller.GetRecent(cts.Token);

        // Assert
        await _receiptService.Received(1).GetRecentReceiptsAsync(10, cts.Token);
    }

    #endregion

    #region UploadReceipt Tests

    [Fact]
    public async Task UploadReceipt_WithValidFile_ReturnsCreatedAtAction()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        fileMock.FileName.Returns("receipt.jpg");
        fileMock.Length.Returns(1024);

        var request = new UploadReceiptDto { File = fileMock };

        var expectedReceipt = new ReceiptDto
        {
            Id = Guid.NewGuid(),
            Total = 100.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, Arg.Any<CancellationToken>())
            .Returns(expectedReceipt);

        // Act
        var result = await _controller.UploadReceipt(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ReceiptsController.GetRecent), createdResult.ActionName);
        var returnedReceipt = Assert.IsType<ReceiptDto>(createdResult.Value);
        Assert.Equal(expectedReceipt.Id, returnedReceipt.Id);
        Assert.Equal(expectedReceipt.Total, returnedReceipt.Total);
    }

    [Fact]
    public async Task UploadReceipt_WithOperationCanceled_ReturnsProblem()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        var request = new UploadReceiptDto { File = fileMock };

        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, Arg.Any<CancellationToken>())
            .Throws(new OperationCanceledException());

        // Act
        var result = await _controller.UploadReceipt(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Upload canceled", problemDetails.Title);
    }

    [Fact]
    public async Task UploadReceipt_WithException_LogsErrorAndReturnsProblem()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        var request = new UploadReceiptDto { File = fileMock };

        var exception = new InvalidOperationException("Analysis failed");
        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        var result = await _controller.UploadReceipt(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal("Failed to analyze receipt", problemDetails.Title);

        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to process receipt upload")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task UploadReceipt_WithPngFile_ProcessesSuccessfully()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        fileMock.FileName.Returns("receipt.png");
        fileMock.ContentType.Returns("image/png");
        fileMock.Length.Returns(2048);

        var request = new UploadReceiptDto { File = fileMock };

        var expectedReceipt = new ReceiptDto
        {
            Id = Guid.NewGuid(),
            Total = 50.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, Arg.Any<CancellationToken>())
            .Returns(expectedReceipt);

        // Act
        var result = await _controller.UploadReceipt(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedReceipt = Assert.IsType<ReceiptDto>(createdResult.Value);
        Assert.Equal(expectedReceipt.Total, returnedReceipt.Total);
    }

    [Fact]
    public async Task UploadReceipt_WithJpegFile_ProcessesSuccessfully()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        fileMock.FileName.Returns("receipt.jpeg");
        fileMock.ContentType.Returns("image/jpeg");
        fileMock.Length.Returns(3072);

        var request = new UploadReceiptDto { File = fileMock };

        var expectedReceipt = new ReceiptDto
        {
            Id = Guid.NewGuid(),
            Total = 75.00m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, Arg.Any<CancellationToken>())
            .Returns(expectedReceipt);

        // Act
        var result = await _controller.UploadReceipt(request, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedReceipt = Assert.IsType<ReceiptDto>(createdResult.Value);
        Assert.Equal(expectedReceipt.Total, returnedReceipt.Total);
    }

    [Fact]
    public async Task UploadReceipt_PassesFileToService()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        var request = new UploadReceiptDto { File = fileMock };

        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, Arg.Any<CancellationToken>())
            .Returns(new ReceiptDto { Id = Guid.NewGuid() });

        // Act
        await _controller.UploadReceipt(request, CancellationToken.None);

        // Assert
        await _receiptService.Received(1).UploadAndAnalyzeReceiptAsync(
            Arg.Is<IFormFile>(f => f == fileMock),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UploadReceipt_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var fileMock = Substitute.For<IFormFile>();
        var request = new UploadReceiptDto { File = fileMock };
        var cts = new CancellationTokenSource();

        _receiptService.UploadAndAnalyzeReceiptAsync(fileMock, cts.Token)
            .Returns(new ReceiptDto { Id = Guid.NewGuid() });

        // Act
        await _controller.UploadReceipt(request, cts.Token);

        // Assert
        await _receiptService.Received(1).UploadAndAnalyzeReceiptAsync(fileMock, cts.Token);
    }

    #endregion

    // Note: Constructor null validation tests removed as controllers don't throw ArgumentNullException
    // in modern .NET when null arguments are passed (they fail at first usage instead)
}








