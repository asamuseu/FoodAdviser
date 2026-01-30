using AutoMapper;
using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Application.Options;
using FoodAdviser.Domain.Enums;
using FoodAdviser.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service for generating recipe suggestions based on available inventory.
/// </summary>
public class RecipeSuggestionService : IRecipeSuggestionService
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IOpenAiService _openAiService;
    private readonly IMapper _mapper;
    private readonly ILogger<RecipeSuggestionService> _logger;
    private readonly RecipeSuggestionOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeSuggestionService"/> class.
    /// </summary>
    public RecipeSuggestionService(
        IFoodItemRepository foodItemRepository,
        IRecipeRepository recipeRepository,
        IOpenAiService openAiService,
        IMapper mapper,
        ILogger<RecipeSuggestionService> logger,
        IOptions<RecipeSuggestionOptions> options)
    {
        _foodItemRepository = foodItemRepository;
        _recipeRepository = recipeRepository;
        _openAiService = openAiService;
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
        _logger.LogInformation(
            "Generating recipe suggestions for DishType={DishType}, NumberOfPersons={NumberOfPersons}",
            dishType, numberOfPersons);

        // Step 1: Retrieve all available products with quantity > 0
        var availableItems = await _foodItemRepository.GetAvailableItemsAsync(ct);

        if (availableItems.Count == 0)
        {
            _logger.LogWarning("No available food items found in inventory");
            throw new InvalidOperationException(
                "No suitable recipes could be generated. Your inventory is empty or all items have zero quantity.");
        }

        _logger.LogInformation("Found {Count} available food items in inventory", availableItems.Count);

        // Step 2: Call OpenAI to generate recipes
        var recipeCount = _options.DefaultRecipeCount;
        var generatedRecipes = await _openAiService.GenerateRecipesAsync(
            availableItems,
            dishType,
            numberOfPersons,
            recipeCount,
            ct);

        if (generatedRecipes.Count == 0)
        {
            _logger.LogWarning(
                "OpenAI could not generate any recipes for DishType={DishType} with available ingredients",
                dishType);
            throw new InvalidOperationException(
                $"No suitable recipes could be generated for {dishType} with the available ingredients. " +
                "Try adding more ingredients to your inventory or selecting a different dish type.");
        }

        _logger.LogInformation("OpenAI generated {Count} recipes", generatedRecipes.Count);

        // Step 3: Save recipes to the database
        var savedRecipes = await _recipeRepository.AddRangeAsync(generatedRecipes, ct);

        _logger.LogInformation("Saved {Count} recipes to database", savedRecipes.Count);

        // Step 4: Map to DTOs and return
        var recipeDtos = _mapper.Map<List<RecipeDto>>(savedRecipes);
        return recipeDtos;
    }
}
