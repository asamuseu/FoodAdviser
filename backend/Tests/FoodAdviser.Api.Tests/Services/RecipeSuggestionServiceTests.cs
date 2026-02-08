using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using AutoMapper;
using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Application.Options;
using FoodAdviser.Application.Services;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Enums;
using FoodAdviser.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FoodAdviser.Api.Tests.Services;

public class RecipeSuggestionServiceTests
{
    private readonly IFixture _fixture;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IAiRecipeServiceFactory _aiRecipeServiceFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<RecipeSuggestionService> _logger;
    private readonly IOptions<RecipeSuggestionOptions> _options;
    private readonly IAiRecipeService _aiRecipeService;
    private readonly RecipeSuggestionService _sut;

    public RecipeSuggestionServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _foodItemRepository = _fixture.Freeze<IFoodItemRepository>();
        _recipeRepository = _fixture.Freeze<IRecipeRepository>();
        _aiRecipeServiceFactory = _fixture.Freeze<IAiRecipeServiceFactory>();
        _currentUserService = _fixture.Freeze<ICurrentUserService>();
        _mapper = _fixture.Freeze<IMapper>();
        _logger = _fixture.Freeze<ILogger<RecipeSuggestionService>>();
        _options = _fixture.Freeze<IOptions<RecipeSuggestionOptions>>();
        _aiRecipeService = _fixture.Freeze<IAiRecipeService>();

        // Setup default options
        _options.Value.Returns(new RecipeSuggestionOptions { DefaultRecipeCount = 3 });

        // Setup AI service factory
        _aiRecipeServiceFactory.GetService().Returns(_aiRecipeService);
        _aiRecipeService.ProviderName.Returns("TestAI");

        _sut = new RecipeSuggestionService(
            _foodItemRepository,
            _recipeRepository,
            _aiRecipeServiceFactory,
            _currentUserService,
            _mapper,
            _logger,
            _options);
    }

    [Theory, AutoData]
    public async Task GenerateRecipeSuggestionsAsync_WithValidIngredients_ShouldReturnRecipeDtos(
        Guid userId,
        DishType dishType,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        var availableItems = _fixture.Build<FoodItem>()
            .With(f => f.UserId, userId)
            .With(f => f.Quantity, () => _fixture.Create<decimal>() + 1) // Ensure quantity > 0
            .CreateMany(5)
            .ToList();

        var generatedRecipes = _fixture.Build<Recipe>()
            .With(r => r.UserId, userId)
            .With(r => r.DishType, dishType)
            .CreateMany(3)
            .ToList();

        var savedRecipes = generatedRecipes.ToList(); // Simulate saved recipes

        var recipeDtos = generatedRecipes.Select(r => _fixture.Build<RecipeDto>()
            .With(dto => dto.Id, r.Id)
            .With(dto => dto.DishType, r.DishType)
            .Create()).ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(availableItems);
        _aiRecipeService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken).Returns(generatedRecipes);
        _recipeRepository.AddRangeAsync(Arg.Any<List<Recipe>>(), cancellationToken).Returns(savedRecipes);
        _mapper.Map<List<RecipeDto>>(savedRecipes).Returns(recipeDtos);

        // Act
        var result = await _sut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, dto => Assert.Equal(dishType, dto.DishType));

        await _aiRecipeService.Received(1).GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken);

        await _recipeRepository.Received(1).AddRangeAsync(
            Arg.Is<List<Recipe>>(recipes => recipes.All(r => r.UserId == userId)),
            cancellationToken);
    }

    [Theory, AutoData]
    public async Task GenerateRecipeSuggestionsAsync_WithNoAvailableItems_ShouldThrowInvalidOperationException(
        Guid userId,
        DishType dishType,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(new List<FoodItem>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken));

        Assert.Contains("No suitable recipes could be generated. Your inventory is empty", exception.Message);

        await _aiRecipeService.DidNotReceive().GenerateRecipesAsync(
            Arg.Any<List<FoodItem>>(),
            Arg.Any<DishType>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoData]
    public async Task GenerateRecipeSuggestionsAsync_WhenAiServiceReturnsNoRecipes_ShouldThrowInvalidOperationException(
        Guid userId,
        DishType dishType,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        var availableItems = _fixture.Build<FoodItem>()
            .With(f => f.UserId, userId)
            .With(f => f.Quantity, 5m)
            .CreateMany(3)
            .ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(availableItems);
        _aiRecipeService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken).Returns(new List<Recipe>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken));

        Assert.Contains($"No suitable recipes could be generated for {dishType}", exception.Message);

        await _recipeRepository.DidNotReceive().AddRangeAsync(
            Arg.Any<List<Recipe>>(),
            Arg.Any<CancellationToken>());
    }

    [Theory, AutoData]
    public async Task GenerateRecipeSuggestionsAsync_WhenRepositoryFails_ShouldPropagateException(
        Guid userId,
        DishType dishType,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        var availableItems = _fixture.CreateMany<FoodItem>(3).ToList();
        var generatedRecipes = _fixture.CreateMany<Recipe>(2).ToList();
        var expectedException = new InvalidOperationException("Database error");

        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(availableItems);
        _aiRecipeService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken).Returns(generatedRecipes);
        _recipeRepository.AddRangeAsync(Arg.Any<List<Recipe>>(), cancellationToken).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken));

        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Theory, AutoData]
    public async Task GenerateRecipeSuggestionsAsync_ShouldSetUserIdOnAllGeneratedRecipes(
        Guid userId,
        DishType dishType,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        var availableItems = _fixture.CreateMany<FoodItem>(3).ToList();
        var generatedRecipes = _fixture.Build<Recipe>()
            .With(r => r.UserId, Guid.NewGuid()) // Different userId initially
            .CreateMany(2)
            .ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(availableItems);
        _aiRecipeService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken).Returns(generatedRecipes);
        _recipeRepository.AddRangeAsync(Arg.Any<List<Recipe>>(), cancellationToken)
            .Returns(callInfo => callInfo.Arg<List<Recipe>>());
        _mapper.Map<List<RecipeDto>>(Arg.Any<List<Recipe>>()).Returns(new List<RecipeDto>());

        // Act
        await _sut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken);

        // Assert
        await _recipeRepository.Received(1).AddRangeAsync(
            Arg.Is<List<Recipe>>(recipes => recipes.All(r => r.UserId == userId)),
            cancellationToken);
    }

    [Theory]
    [InlineAutoData(DishType.Salad)]
    [InlineAutoData(DishType.Soup)]
    [InlineAutoData(DishType.MainCourse)]
    [InlineAutoData(DishType.Dessert)]
    [InlineAutoData(DishType.Appetizer)]
    public async Task GenerateRecipeSuggestionsAsync_WithDifferentDishTypes_ShouldPassCorrectDishType(
        DishType dishType,
        Guid userId,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        var availableItems = _fixture.CreateMany<FoodItem>(3).ToList();
        var generatedRecipes = _fixture.Build<Recipe>()
            .With(r => r.DishType, dishType)
            .CreateMany(1)
            .ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(availableItems);
        _aiRecipeService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken).Returns(generatedRecipes);
        _recipeRepository.AddRangeAsync(Arg.Any<List<Recipe>>(), cancellationToken).Returns(generatedRecipes);
        _mapper.Map<List<RecipeDto>>(generatedRecipes).Returns(new List<RecipeDto>());

        // Act
        await _sut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken);

        // Assert
        await _aiRecipeService.Received(1).GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            3,
            cancellationToken);
    }

    [Theory, AutoData]
    public async Task GenerateRecipeSuggestionsAsync_ShouldUseConfiguredRecipeCount(
        Guid userId,
        int numberOfPersons,
        CancellationToken cancellationToken)
    {
        // Arrange
        const int configuredRecipeCount = 5;
        const DishType dishType = DishType.MainCourse;

        // Create a new options instance with custom recipe count
        var customOptions = Substitute.For<IOptions<RecipeSuggestionOptions>>();
        customOptions.Value.Returns(new RecipeSuggestionOptions { DefaultRecipeCount = configuredRecipeCount });

        // Create a new service instance with the custom options
        var customSut = new RecipeSuggestionService(
            _foodItemRepository,
            _recipeRepository,
            _aiRecipeServiceFactory,
            _currentUserService,
            _mapper,
            _logger,
            customOptions);

        var availableItems = _fixture.CreateMany<FoodItem>(3).ToList();
        var generatedRecipes = _fixture.CreateMany<Recipe>(configuredRecipeCount).ToList();

        _currentUserService.GetRequiredUserId().Returns(userId);
        _foodItemRepository.GetAvailableItemsAsync(userId, cancellationToken).Returns(availableItems);
        _aiRecipeService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            configuredRecipeCount,
            cancellationToken).Returns(generatedRecipes);
        _recipeRepository.AddRangeAsync(Arg.Any<List<Recipe>>(), cancellationToken).Returns(generatedRecipes);
        _mapper.Map<List<RecipeDto>>(generatedRecipes).Returns(new List<RecipeDto>());

        // Act
        await customSut.GenerateRecipeSuggestionsAsync(dishType, numberOfPersons, cancellationToken);

        // Assert
        await _aiRecipeService.Received(1).GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            configuredRecipeCount,
            cancellationToken);
    }
}

