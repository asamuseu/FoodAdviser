using AutoMapper;
using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Application.Options;
using FoodAdviser.Domain.Enums;
using FoodAdviser.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service for generating recipe suggestions based on available inventory.
/// All operations are scoped to the current authenticated user.
/// </summary>
public class RecipeSuggestionService : IRecipeSuggestionService
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IAiRecipeServiceFactory _aiRecipeServiceFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<RecipeSuggestionService> _logger;
    private readonly RecipeSuggestionOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeSuggestionService"/> class.
    /// </summary>
    public RecipeSuggestionService(
        IFoodItemRepository foodItemRepository,
        IRecipeRepository recipeRepository,
        IAiRecipeServiceFactory aiRecipeServiceFactory,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<RecipeSuggestionService> logger,
        IOptions<RecipeSuggestionOptions> options)
    {
        _foodItemRepository = foodItemRepository;
        _recipeRepository = recipeRepository;
        _aiRecipeServiceFactory = aiRecipeServiceFactory;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RecipeDto>> GenerateRecipeSuggestionsAsync(
        DishType dishType,
        int numberOfPersons,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.GetRequiredUserId();

        _logger.LogInformation(
            "User {UserId} generating recipe suggestions for DishType={DishType}, NumberOfPersons={NumberOfPersons}",
            userId, dishType, numberOfPersons);

        // Step 1: Retrieve all available products with quantity > 0 for this user
        var availableItems = await _foodItemRepository.GetAvailableItemsAsync(userId, ct);

        if (availableItems.Count == 0)
        {
            _logger.LogWarning("No available food items found in inventory for user {UserId}", userId);
            throw new InvalidOperationException(
                "No suitable recipes could be generated. Your inventory is empty or all items have zero quantity.");
        }

        _logger.LogInformation("Found {Count} available food items in inventory for user {UserId}", availableItems.Count, userId);

        // Step 2: Get the configured AI service and generate recipes
        var aiService = _aiRecipeServiceFactory.GetService();
        _logger.LogInformation("Using AI provider: {Provider}", aiService.ProviderName);

        var recipeCount = _options.DefaultRecipeCount;
        var generatedRecipes = await aiService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            recipeCount,
            ct);

        if (generatedRecipes.Count == 0)
        {
            _logger.LogWarning(
                "AI provider {Provider} could not generate any recipes for DishType={DishType} with available ingredients",
                aiService.ProviderName, dishType);
            throw new InvalidOperationException(
                $"No suitable recipes could be generated for {dishType} with the available ingredients. " +
                "Try adding more ingredients to your inventory or selecting a different dish type.");
        }

        _logger.LogInformation("{Provider} generated {Count} recipes", aiService.ProviderName, generatedRecipes.Count);

        // Step 3: Set user ID on each recipe and save to the database
        foreach (var recipe in generatedRecipes)
        {
            recipe.UserId = userId;
        }

        var savedRecipes = await _recipeRepository.AddRangeAsync(generatedRecipes, ct);

        _logger.LogInformation("Saved {Count} recipes to database for user {UserId}", savedRecipes.Count, userId);

        // Step 4: Map to DTOs and return
        var recipeDtos = _mapper.Map<List<RecipeDto>>(savedRecipes);
        return recipeDtos;
    }
}
