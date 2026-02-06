using AutoMapper;
using FoodAdviser.Application.DTOs.Receipts;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service implementation for receipt operations.
/// All operations are scoped to the current authenticated user.
/// </summary>
public class ReceiptService : IReceiptService
{
    private readonly IReceiptAnalyzerService _analyzer;
    private readonly IReceiptRepository _repository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly StorageOptions _storageOptions;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        IReceiptAnalyzerService analyzer,
        IReceiptRepository repository,
        IFoodItemRepository foodItemRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        IOptions<StorageOptions> storageOptions,
        ILogger<ReceiptService> logger)
    {
        _analyzer = analyzer;
        _repository = repository;
        _foodItemRepository = foodItemRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _storageOptions = storageOptions.Value;
        _logger = logger;
    }

    public async Task<ReceiptDto> UploadAndAnalyzeReceiptAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        
        var baseTemp = _storageOptions.ReceiptTempPath ?? Path.Combine(Path.GetTempPath(), "FoodAdviser", "receipts");
        Directory.CreateDirectory(baseTemp);
        var ext = Path.GetExtension(file.FileName ?? string.Empty);
        var tempFile = Path.Combine(baseTemp, $"{Guid.NewGuid()}{ext}");

        try
        {
            await using (var fs = new FileStream(tempFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(fs, cancellationToken);
            }

            _logger.LogInformation("User {UserId} analyzing receipt from file: {FileName}", userId, file.FileName);
            var receipt = await _analyzer.AnalyzeAsync(tempFile, cancellationToken);
            
            // Set the user ID on the receipt
            receipt.UserId = userId;
            
            var added = await _repository.AddAsync(receipt, cancellationToken);
            _logger.LogInformation("Receipt {ReceiptId} successfully persisted for user {UserId}", added.Id, userId);
            
            var receiptDto = _mapper.Map<ReceiptDto>(added);
            
            // Update food inventory based on receipt items
            await UpdateFoodInventoryAsync(receiptDto.Items, userId, cancellationToken);
            
            return receiptDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload and analyze receipt from file: {FileName}", file.FileName);
            throw;
        }
        finally
        {
            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    _logger.LogDebug("Temporary file deleted: {TempFile}", tempFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete temporary file: {TempFile}", tempFile);
            }
        }
    }

    public async Task<IReadOnlyList<ReceiptDto>> GetRecentReceiptsAsync(int count, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        _logger.LogInformation("User {UserId} retrieving {Count} recent receipts", userId, count);
        var receipts = await _repository.GetRecentAsync(count, userId, cancellationToken);
        return receipts.Select(r => _mapper.Map<ReceiptDto>(r)).ToList();
    }

    /// <summary>
    /// Updates the food inventory based on receipt line items.
    /// Adds new items or increments quantity for existing items.
    /// </summary>
    /// <param name="items">The receipt line items to process.</param>
    /// <param name="userId">The user ID to scope the inventory operations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task UpdateFoodInventoryAsync(IReadOnlyList<ReceiptLineItemDto> items, Guid userId, CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            try
            {
                var existingFoodItem = await _foodItemRepository.GetByNameAsync(item.Name, userId, cancellationToken);

                if (existingFoodItem is not null)
                {
                    // Item exists - increment quantity
                    existingFoodItem.Quantity += item.Quantity;
                    
                    // Update unit if provided and existing item has no unit
                    if (!string.IsNullOrWhiteSpace(item.Unit) && string.IsNullOrWhiteSpace(existingFoodItem.Unit))
                    {
                        existingFoodItem.Unit = item.Unit;
                    }
                    
                    await _foodItemRepository.UpdateAsync(existingFoodItem, cancellationToken);
                    _logger.LogInformation(
                        "Updated food item '{Name}' quantity for user {UserId}. New quantity: {Quantity}",
                        existingFoodItem.Name,
                        userId,
                        existingFoodItem.Quantity);
                }
                else
                {
                    // Item does not exist - add new
                    var newFoodItem = new FoodItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = item.Name,
                        Quantity = item.Quantity,
                        Unit = item.Unit ?? string.Empty
                    };
                    
                    await _foodItemRepository.AddAsync(newFoodItem, cancellationToken);
                    _logger.LogInformation(
                        "Added new food item '{Name}' to inventory for user {UserId} with quantity: {Quantity}",
                        newFoodItem.Name,
                        userId,
                        newFoodItem.Quantity);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to update inventory for item '{Name}'. Continuing with remaining items.",
                    item.Name);
            }
        }
    }
}
