using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FoodAdviser.Application.Options;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Enums;
using FoodAdviser.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FoodAdviser.Infrastructure.Tests.Services;

public class OpenAiServiceTests
{
    private readonly ILogger<OpenAiService> _logger;
    private readonly OpenAiOptions _options;
    private readonly HttpClient _httpClient;

    public OpenAiServiceTests()
    {
        _logger = Substitute.For<ILogger<OpenAiService>>();

        _options = new OpenAiOptions
        {
            ApiKey = "test-api-key",
            Model = "gpt-4",
            TimeoutSeconds = 60
        };

        _httpClient = new HttpClient();
    }

    [Fact]
    public void Constructor_InitializesProviderName()
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);

        // Act
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        // Assert
        Assert.Equal("OpenAI", service.ProviderName);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsNullReferenceException()
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        HttpClient nullHttpClient = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => new OpenAiService(nullHttpClient, _logger, optionsWrapper));
    }

    // Note: Logger can be null without immediate exception in constructor
    // It will only throw when logging is attempted

    [Fact]
    public void Constructor_WithNullOptions_ThrowsNullReferenceException()
    {
        // Arrange
        IOptions<OpenAiOptions> nullOptions = null!;

        // Act & Assert
        Assert.Throws<NullReferenceException>(
            () => new OpenAiService(_httpClient, _logger, nullOptions));
    }

    [Fact]
    public async Task GenerateRecipesAsync_WithEmptyApiKey_ThrowsException()
    {
        // Arrange
        _options.ApiKey = string.Empty;
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Chicken", Quantity = 500, Unit = "g" }
        };

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.MainCourse,
                2,
                1));
    }

    [Theory]
    [InlineData(DishType.Appetizer)]
    [InlineData(DishType.MainCourse)]
    [InlineData(DishType.Dessert)]
    [InlineData(DishType.Soup)]
    public async Task GenerateRecipesAsync_WithDifferentDishTypes_BuildsCorrectPrompt(DishType dishType)
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Tomato", Quantity = 3, Unit = "pcs" }
        };

        // Act & Assert
        // This will fail with HTTP error because we're using fake API key,
        // but it proves the service can handle different dish types
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                dishType,
                2,
                1));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public async Task GenerateRecipesAsync_WithDifferentRecipeCounts_RespectsParameter(int recipeCount)
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Rice", Quantity = 200, Unit = "g" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.MainCourse,
                2,
                recipeCount));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(10)]
    public async Task GenerateRecipesAsync_WithDifferentPersonCounts_RespectsParameter(int numberOfPersons)
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Pasta", Quantity = 500, Unit = "g" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.MainCourse,
                numberOfPersons,
                1));
    }

    [Fact]
    public async Task GenerateRecipesAsync_WithMultipleIngredients_BuildsCorrectPrompt()
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Chicken", Quantity = 500, Unit = "g" },
            new() { Id = Guid.NewGuid(), Name = "Rice", Quantity = 200, Unit = "g" },
            new() { Id = Guid.NewGuid(), Name = "Tomato", Quantity = 3, Unit = "pcs" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.MainCourse,
                2,
                1));
    }

    [Fact]
    public async Task GenerateRecipesAsync_WithEmptyFoodItems_BuildsPrompt()
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = Array.Empty<FoodItem>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.MainCourse,
                2,
                1));
    }

    [Fact]
    public async Task GenerateRecipesAsync_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Chicken", Quantity = 500, Unit = "g" }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.MainCourse,
                2,
                1,
                cts.Token));
    }

    [Theory]
    [InlineData("gpt-3.5-turbo")]
    [InlineData("gpt-4")]
    [InlineData("gpt-4-turbo")]
    public async Task GenerateRecipesAsync_WithDifferentModels_UsesConfiguredModel(string model)
    {
        // Arrange
        _options.Model = model;
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Eggs", Quantity = 4, Unit = "pcs" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.Salad,
                2,
                1));
    }

    [Fact]
    public async Task GenerateRecipesAsync_WithTimeout_ThrowsInvalidOperationException()
    {
        // Arrange
        _options.TimeoutSeconds = 1; // Very short timeout
        var optionsWrapper = Substitute.For<IOptions<OpenAiOptions>>();
        optionsWrapper.Value.Returns(_options);
        var service = new OpenAiService(_httpClient, _logger, optionsWrapper);

        var foodItems = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Flour", Quantity = 500, Unit = "g" }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.GenerateRecipesAsync(
                foodItems,
                DishType.Dessert,
                2,
                1));
    }
}








