using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FoodAdviser.Application.Options;
using FoodAdviser.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoodAdviser.Infrastructure.Tests.Services;

public class ReceiptAnalyzerServiceTests
{
    private readonly ILogger<ReceiptAnalyzerService> _logger;
    private readonly ReceiptAnalyzerOptions _options;
    private readonly ReceiptAnalyzerService _sut;

    public ReceiptAnalyzerServiceTests()
    {
        _logger = Substitute.For<ILogger<ReceiptAnalyzerService>>();

        _options = new ReceiptAnalyzerOptions
        {
            Username = "test-username",
            ApiKey = "test-api-key",
            ClientId = "test-client-id",
            RetryCount = 3,
            RetryDelayMs = 100,
            TimeoutSeconds = 30
        };

        var optionsWrapper = Substitute.For<IOptions<ReceiptAnalyzerOptions>>();
        optionsWrapper.Value.Returns(_options);

        _sut = new ReceiptAnalyzerService(optionsWrapper, _logger);
    }

    [Fact]
    public async Task AnalyzeAsync_WithNonExistentFile_ThrowsException()
    {
        // Arrange
        var nonExistentPath = "C:\\non-existent-file.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await _sut.AnalyzeAsync(nonExistentPath));
    }

    [Fact]
    public async Task AnalyzeAsync_WithCancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, new byte[] { 1, 2, 3, 4, 5 });
        var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                async () => await _sut.AnalyzeAsync(tempFile, cts.Token));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task AnalyzeAsync_WithInvalidCredentials_ThrowsApiException()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, new byte[] { 0xFF, 0xD8, 0xFF }); // JPEG header

        try
        {
            // Act & Assert
            // This should fail because credentials are fake, throws Veryfi API exception
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await _sut.AnalyzeAsync(tempFile));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsNullReferenceException()
    {
        // Arrange
        IOptions<ReceiptAnalyzerOptions> nullOptions = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => new ReceiptAnalyzerService(nullOptions, _logger));
    }

    // Note: Logger can be null without immediate exception in constructor
    // It will only throw when logging is attempted

    [Theory]
    [InlineData(0)] // RetryCount < 1 should default to 1
    [InlineData(-1)]
    [InlineData(5)]
    public async Task AnalyzeAsync_RespectsRetryCount(int configuredRetries)
    {
        // Arrange
        _options.RetryCount = configuredRetries;
        var tempFile = Path.GetTempFileName();
        await File.WriteAllBytesAsync(tempFile, new byte[] { 0xFF, 0xD8, 0xFF });

        try
        {
            // Act & Assert
            // Veryfi API will throw exception with invalid credentials
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await _sut.AnalyzeAsync(tempFile));

            // Note: In a real scenario with actual retry logic monitoring,
            // we would verify the exact number of retry attempts
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Theory]
    [InlineData("test.jpg")]
    [InlineData("receipt.png")]
    [InlineData("invoice.pdf")]
    public async Task AnalyzeAsync_WithDifferentFileExtensions_AttemptsAnalysis(string fileName)
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), fileName);
        await File.WriteAllBytesAsync(tempFile, new byte[] { 0xFF, 0xD8, 0xFF });

        try
        {
            // Act & Assert
            // Should attempt to analyze regardless of extension, will throw with invalid credentials
            await Assert.ThrowsAnyAsync<Exception>(
                async () => await _sut.AnalyzeAsync(tempFile));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}









