using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using FoodAdviser.Application.Services;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FoodAdviser.Api.Tests.Services;

public class InventoryServiceTests
{
    private readonly IFixture _fixture;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InventoryService> _logger;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _recipeRepository = _fixture.Freeze<IRecipeRepository>();
        _foodItemRepository = _fixture.Freeze<IFoodItemRepository>();
        _currentUserService = _fixture.Freeze<ICurrentUserService>();
        _logger = _fixture.Freeze<ILogger<InventoryService>>();
        _sut = new InventoryService(_recipeRepository, _foodItemRepository, _currentUserService, _logger);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WithValidRecipes_ShouldUpdateInventoryAndReturnSuccess(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var recipeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var recipes = recipeIds.Select(id => _fixture.Build<Recipe>()
            .With(r => r.Id, id)
            .With(r => r.UserId, userId)
            .With(r => r.Ingredients, new List<Ingredient>
            {
                new() { Name = "Tomato", Quantity = 2, Unit = "pcs" },
                new() { Name = "Onion", Quantity = 1, Unit = "pcs" }
            })
            .Create()).ToList();

        var foodItems = new List<FoodItem>
        {
            new() { Name = "Tomato", Quantity = 10, Unit = "pcs", UserId = userId }, // Sufficient quantity
            new() { Name = "Onion", Quantity = 5, Unit = "pcs", UserId = userId }     // Sufficient quantity
        };

        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).Returns(recipes);
        _foodItemRepository.GetByNamesAsync(Arg.Any<List<string>>(), userId, cancellationToken).Returns(foodItems);

        // Act
        var result = await _sut.ConfirmRecipesAsync(recipeIds, cancellationToken);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(recipes.Count, result.ConfirmedRecipesCount);
        Assert.NotEmpty(result.InventoryUpdates);
        
        await _foodItemRepository.Received(1).UpdateRangeAsync(
            Arg.Is<List<FoodItem>>(items => items.All(item => item.Quantity >= 0)), 
            cancellationToken);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WithNoRecipesFound_ShouldThrowInvalidOperationException(
        Guid userId,
        List<Guid> recipeIds,
        CancellationToken cancellationToken)
    {
        // Arrange
        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).Returns(new List<Recipe>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ConfirmRecipesAsync(recipeIds, cancellationToken));
        
        Assert.Equal("No recipes found for the provided IDs.", exception.Message);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WithMissingRecipes_ShouldThrowInvalidOperationException(
        Guid userId,
        List<Guid> recipeIds,
        CancellationToken cancellationToken)
    {
        // Arrange
        var partialRecipes = _fixture.Build<Recipe>()
            .With(r => r.Id, recipeIds.First())
            .With(r => r.UserId, userId)
            .CreateMany(1)
            .ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).Returns(partialRecipes);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ConfirmRecipesAsync(recipeIds, cancellationToken));
        
        Assert.Contains("The following recipe IDs were not found:", exception.Message);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WithMissingIngredients_ShouldThrowInvalidOperationException(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var recipeIds = new List<Guid> { Guid.NewGuid() };
        var recipes = _fixture.Build<Recipe>()
            .With(r => r.Id, recipeIds.First())
            .With(r => r.UserId, userId)
            .With(r => r.Ingredients, new List<Ingredient>
            {
                new() { Name = "MissingIngredient", Quantity = 1, Unit = "pcs" }
            })
            .CreateMany(1)
            .ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).Returns(recipes);
        _foodItemRepository.GetByNamesAsync(Arg.Any<List<string>>(), userId, cancellationToken).Returns(new List<FoodItem>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ConfirmRecipesAsync(recipeIds, cancellationToken));
        
        Assert.Contains("The following ingredients are not in your inventory:", exception.Message);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WithInsufficientIngredients_ShouldThrowInvalidOperationException(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Arrange
        var recipeIds = new List<Guid> { Guid.NewGuid() };
        var recipes = _fixture.Build<Recipe>()
            .With(r => r.Id, recipeIds.First())
            .With(r => r.UserId, userId)
            .With(r => r.Ingredients, new List<Ingredient>
            {
                new() { Name = "Tomato", Quantity = 10, Unit = "pcs" }
            })
            .CreateMany(1)
            .ToList();

        var foodItems = new List<FoodItem>
        {
            new() { Name = "Tomato", Quantity = 5, Unit = "pcs", UserId = userId }
        };

        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).Returns(recipes);
        _foodItemRepository.GetByNamesAsync(Arg.Any<List<string>>(), userId, cancellationToken).Returns(foodItems);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ConfirmRecipesAsync(recipeIds, cancellationToken));
        
        Assert.Contains("Insufficient quantities for the following ingredients:", exception.Message);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WithMultipleRecipesUsingSameIngredient_ShouldAggregateQuantities(
        Guid userId,
        List<Guid> recipeIds,
        CancellationToken cancellationToken)
    {
        // Arrange
        var recipes = recipeIds.Select(id => _fixture.Build<Recipe>()
            .With(r => r.Id, id)
            .With(r => r.UserId, userId)
            .With(r => r.Ingredients, new List<Ingredient>
            {
                new() { Name = "Tomato", Quantity = 2, Unit = "pcs" }
            })
            .Create()).ToList();

        var expectedTotalQuantity = recipes.Count * 2; // 2 tomatoes per recipe
        var foodItems = new List<FoodItem>
        {
            new() { Name = "Tomato", Quantity = expectedTotalQuantity + 1, Unit = "pcs", UserId = userId }
        };

        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).Returns(recipes);
        _foodItemRepository.GetByNamesAsync(Arg.Any<List<string>>(), userId, cancellationToken).Returns(foodItems);

        // Act
        var result = await _sut.ConfirmRecipesAsync(recipeIds, cancellationToken);

        // Assert
        Assert.True(result.Success);
        var tomatoUpdate = result.InventoryUpdates.Single(u => u.ProductName == "Tomato");
        Assert.Equal(expectedTotalQuantity, tomatoUpdate.UsedQuantity);
        Assert.Equal(1, tomatoUpdate.NewQuantity);
    }

    [Theory, AutoData]
    public async Task ConfirmRecipesAsync_WhenRepositoryThrows_ShouldPropagateException(
        Guid userId,
        List<Guid> recipeIds,
        CancellationToken cancellationToken)
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");
        _currentUserService.GetRequiredUserId().Returns(userId);
        _recipeRepository.GetByIdsAsync(recipeIds, userId, cancellationToken).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.ConfirmRecipesAsync(recipeIds, cancellationToken));
        
        Assert.Equal(expectedException.Message, exception.Message);
    }
}



