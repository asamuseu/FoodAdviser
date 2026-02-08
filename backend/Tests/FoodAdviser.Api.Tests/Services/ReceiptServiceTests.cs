using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using AutoMapper;
using FoodAdviser.Application.DTOs.Receipts;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FoodAdviser.Api.Tests.Services;

public class ReceiptServiceTests : IDisposable
{
    private readonly IFixture _fixture;
    private readonly IReceiptAnalyzerService _analyzer;
    private readonly IReceiptRepository _repository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly IOptions<StorageOptions> _storageOptions;
    private readonly ILogger<ReceiptService> _logger;
    private readonly ReceiptService _sut;
    private readonly string _tempDirectory;

    public ReceiptServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _analyzer = _fixture.Freeze<IReceiptAnalyzerService>();
        _repository = _fixture.Freeze<IReceiptRepository>();
        _foodItemRepository = _fixture.Freeze<IFoodItemRepository>();
        _currentUserService = _fixture.Freeze<ICurrentUserService>();
        _mapper = _fixture.Freeze<IMapper>();
        _storageOptions = _fixture.Freeze<IOptions<StorageOptions>>();
        _logger = _fixture.Freeze<ILogger<ReceiptService>>();

        // Set up temp directory for tests
        _tempDirectory = Path.Combine(Path.GetTempPath(), "FoodAdviser_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        
        _storageOptions.Value.Returns(new StorageOptions { ReceiptTempPath = _tempDirectory });
        
        _sut = new ReceiptService(_analyzer, _repository, _foodItemRepository, _currentUserService, _mapper, _storageOptions, _logger);
    }

    [Theory, AutoData]
    public async Task UploadAndAnalyzeReceiptAsync_WithValidFile_ShouldReturnReceiptDto(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var fileName = "test_receipt.jpg";
        var fileContent = "fake image content"u8.ToArray();
        
        var formFile = CreateMockFormFile(fileName, fileContent);
        var analyzedReceipt = CreateSampleReceipt(userId);
        var receiptDto = CreateSampleReceiptDto();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _analyzer.AnalyzeAsync(Arg.Any<string>(), cancellationToken).Returns(analyzedReceipt);
        _repository.AddAsync(Arg.Any<Receipt>(), cancellationToken).Returns(analyzedReceipt);
        _mapper.Map<ReceiptDto>(analyzedReceipt).Returns(receiptDto);

        // Mock food inventory operations
        _foodItemRepository.GetByNameAsync(Arg.Any<string>(), userId, cancellationToken).Returns((FoodItem?)null);

        // Act
        var result = await _sut.UploadAndAnalyzeReceiptAsync(formFile, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(receiptDto.Id, result.Id);
        
        await _analyzer.Received(1).AnalyzeAsync(Arg.Any<string>(), cancellationToken);
        await _repository.Received(1).AddAsync(Arg.Is<Receipt>(r => r.UserId == userId), cancellationToken);
        
        // Verify food items were added for each receipt item
        await _foodItemRepository.Received(receiptDto.Items.Count).AddAsync(Arg.Any<FoodItem>(), cancellationToken);
    }

    [Theory, AutoData]
    public async Task UploadAndAnalyzeReceiptAsync_WithExistingFoodItems_ShouldUpdateQuantities(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var formFile = CreateMockFormFile("test.jpg", "content"u8.ToArray());
        var analyzedReceipt = CreateSampleReceipt(userId);
        var receiptDto = CreateSampleReceiptDto();
        
        var existingFoodItem = new FoodItem
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = receiptDto.Items.First().Name,
            Quantity = 5,
            Unit = "pcs"
        };

        _currentUserService.GetRequiredUserId().Returns(userId);
        _analyzer.AnalyzeAsync(Arg.Any<string>(), cancellationToken).Returns(analyzedReceipt);
        _repository.AddAsync(Arg.Any<Receipt>(), cancellationToken).Returns(analyzedReceipt);
        _mapper.Map<ReceiptDto>(analyzedReceipt).Returns(receiptDto);
        _foodItemRepository.GetByNameAsync(receiptDto.Items.First().Name, userId, cancellationToken).Returns(existingFoodItem);
        _foodItemRepository.GetByNameAsync(Arg.Is<string>(name => name != receiptDto.Items.First().Name), userId, cancellationToken).Returns((FoodItem?)null);

        // Act
        var result = await _sut.UploadAndAnalyzeReceiptAsync(formFile, cancellationToken);

        // Assert
        await _foodItemRepository.Received(1).UpdateAsync(
            Arg.Is<FoodItem>(item => 
                item.Name == existingFoodItem.Name && 
                item.Quantity > existingFoodItem.Quantity - receiptDto.Items.First().Quantity), 
            cancellationToken);
        
        // Other items should be added as new
        await _foodItemRepository.Received(receiptDto.Items.Count - 1).AddAsync(Arg.Any<FoodItem>(), cancellationToken);
    }

    [Theory, AutoData]
    public async Task UploadAndAnalyzeReceiptAsync_WhenAnalyzerFails_ShouldThrowException(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var formFile = CreateMockFormFile("test.jpg", "content"u8.ToArray());
        var expectedException = new InvalidOperationException("Analysis failed");

        _currentUserService.GetRequiredUserId().Returns(userId);
        _analyzer.AnalyzeAsync(Arg.Any<string>(), cancellationToken).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.UploadAndAnalyzeReceiptAsync(formFile, cancellationToken));
        
        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Theory, AutoData]
    public async Task UploadAndAnalyzeReceiptAsync_ShouldCleanUpTempFile(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var formFile = CreateMockFormFile("test.jpg", "content"u8.ToArray());
        var analyzedReceipt = CreateSampleReceipt(userId);

        _currentUserService.GetRequiredUserId().Returns(userId);
        _analyzer.AnalyzeAsync(Arg.Any<string>(), cancellationToken).Returns(analyzedReceipt);
        _repository.AddAsync(Arg.Any<Receipt>(), cancellationToken).Returns(analyzedReceipt);
        _mapper.Map<ReceiptDto>(analyzedReceipt).Returns(CreateSampleReceiptDto());
        _foodItemRepository.GetByNameAsync(Arg.Any<string>(), userId, cancellationToken).Returns((FoodItem?)null);

        // Act
        await _sut.UploadAndAnalyzeReceiptAsync(formFile, cancellationToken);

        // Assert - temp files should be cleaned up
        var tempFiles = Directory.GetFiles(_tempDirectory, "*.*", SearchOption.AllDirectories);
        Assert.Empty(tempFiles);
    }

    [Theory, AutoData]
    public async Task GetRecentReceiptsAsync_ShouldReturnMappedReceipts(
        Guid userId,
        int count,
        CancellationToken cancellationToken)
    {
        // Arrange - manually create test data to avoid AutoFixture decimal range issues
        var receipts = Enumerable.Range(0, count).Select(_ => new Receipt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        var receiptDtos = receipts.Select(r => new ReceiptDto
        {
            Id = r.Id,
            CreatedAt = r.CreatedAt,
            Items = new List<ReceiptLineItemDto>
            {
                new() { Name = "Test Item", Quantity = 1.5m, Unit = "kg" }
            }.AsReadOnly(),
            Total = 10.50m
        }).ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _repository.GetRecentAsync(count, userId, cancellationToken).Returns(receipts);
        
        foreach (var (receipt, dto) in receipts.Zip(receiptDtos))
        {
            _mapper.Map<ReceiptDto>(receipt).Returns(dto);
        }

        // Act
        var result = await _sut.GetRecentReceiptsAsync(count, cancellationToken);

        // Assert
        Assert.Equal(count, result.Count);
        Assert.All(result.Zip(receiptDtos), pair => Assert.Equal(pair.Second.Id, pair.First.Id));
    }

    [Theory, AutoData]
    public async Task GetRecentReceiptsAsync_WhenRepositoryFails_ShouldPropagateException(
        Guid userId,
        int count,
        CancellationToken cancellationToken)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");
        _currentUserService.GetRequiredUserId().Returns(userId);
        _repository.GetRecentAsync(count, userId, cancellationToken).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetRecentReceiptsAsync(count, cancellationToken));
        
        Assert.Equal(expectedException.Message, exception.Message);
    }

    private IFormFile CreateMockFormFile(string fileName, byte[] content)
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(fileName);
        formFile.Length.Returns(content.Length);
        
        formFile.CopyToAsync(Arg.Any<Stream>(), Arg.Any<CancellationToken>())
            .Returns(async (callInfo) =>
            {
                var stream = callInfo.Arg<Stream>();
                await stream.WriteAsync(content);
            });

        return formFile;
    }

    private Receipt CreateSampleReceipt(Guid userId)
    {
        return _fixture.Build<Receipt>()
            .With(r => r.UserId, userId)
            .Create();
    }

    private ReceiptDto CreateSampleReceiptDto()
    {
        return new ReceiptDto
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            Items = new List<ReceiptLineItemDto>
            {
                new() { Name = "Apple", Quantity = 3, Unit = "pcs" },
                new() { Name = "Bread", Quantity = 1, Unit = "loaf" }
            }.AsReadOnly(),
            Total = 5.99m
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }
}



