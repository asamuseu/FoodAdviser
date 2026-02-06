using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FoodAdviser.Application.Services;

/// <summary>
/// Service for managing user inventory operations including recipe confirmation.
/// All operations are scoped to the current authenticated user.
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<InventoryService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InventoryService"/> class.
    /// </summary>
    public InventoryService(
        IRecipeRepository recipeRepository,
        IFoodItemRepository foodItemRepository,
        ICurrentUserService currentUserService,
        ILogger<InventoryService> logger)
    {
        _recipeRepository = recipeRepository;
        _foodItemRepository = foodItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ConfirmRecipesResponseDto> ConfirmRecipesAsync(
        IReadOnlyList<Guid> recipeIds,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.GetRequiredUserId();
        _logger.LogInformation("User {UserId} confirming {Count} recipes", userId, recipeIds.Count);

        // Step 1: Retrieve the recipes from the database for this user
        var recipes = await _recipeRepository.GetByIdsAsync(recipeIds, userId, ct);

        if (recipes.Count == 0)
        {
            _logger.LogWarning("No recipes found for the provided IDs");
            throw new InvalidOperationException("No recipes found for the provided IDs.");
        }

        if (recipes.Count != recipeIds.Count)
        {
            var foundIds = recipes.Select(r => r.Id).ToHashSet();
            var missingIds = recipeIds.Where(id => !foundIds.Contains(id)).ToList();
            _logger.LogWarning("Some recipes were not found: {MissingIds}", string.Join(", ", missingIds));
            throw new InvalidOperationException(
                $"The following recipe IDs were not found: {string.Join(", ", missingIds)}");
        }

        // Step 2: Aggregate all ingredients from all recipes
        var ingredientUsage = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        
        foreach (var recipe in recipes)
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredientUsage.TryGetValue(ingredient.Name, out var existingQuantity))
                {
                    ingredientUsage[ingredient.Name] = existingQuantity + ingredient.Quantity;
                }
                else
                {
                    ingredientUsage[ingredient.Name] = ingredient.Quantity;
                }
            }
        }

        _logger.LogInformation("Aggregated {Count} unique ingredients from recipes", ingredientUsage.Count);

        // Step 3: Retrieve the corresponding food items from inventory for this user
        var ingredientNames = ingredientUsage.Keys.ToList();
        var foodItems = await _foodItemRepository.GetByNamesAsync(ingredientNames, userId, ct);

        // Create a lookup for easy access
        var foodItemLookup = foodItems.ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);

        // Step 4: Validate that all ingredients exist and have sufficient quantity
        var missingIngredients = new List<string>();
        var insufficientIngredients = new List<(string Name, decimal Required, decimal Available)>();

        foreach (var (ingredientName, requiredQuantity) in ingredientUsage)
        {
            if (!foodItemLookup.TryGetValue(ingredientName, out var foodItem))
            {
                missingIngredients.Add(ingredientName);
            }
            else if (foodItem.Quantity < requiredQuantity)
            {
                insufficientIngredients.Add((ingredientName, requiredQuantity, foodItem.Quantity));
            }
        }

        if (missingIngredients.Count > 0)
        {
            var missing = string.Join(", ", missingIngredients);
            _logger.LogWarning("Missing ingredients in inventory: {MissingIngredients}", missing);
            throw new InvalidOperationException(
                $"The following ingredients are not in your inventory: {missing}");
        }

        if (insufficientIngredients.Count > 0)
        {
            var insufficient = string.Join("; ", 
                insufficientIngredients.Select(i => 
                    $"{i.Name} (required: {i.Required}, available: {i.Available})"));
            _logger.LogWarning("Insufficient ingredient quantities: {InsufficientIngredients}", insufficient);
            throw new InvalidOperationException(
                $"Insufficient quantities for the following ingredients: {insufficient}");
        }

        // Step 5: Update the inventory quantities
        var inventoryUpdates = new List<InventoryUpdateDto>();
        var itemsToUpdate = new List<Domain.Entities.FoodItem>();

        foreach (var (ingredientName, usedQuantity) in ingredientUsage)
        {
            var foodItem = foodItemLookup[ingredientName];
            var previousQuantity = foodItem.Quantity;
            foodItem.Quantity -= usedQuantity;

            itemsToUpdate.Add(foodItem);
            inventoryUpdates.Add(new InventoryUpdateDto
            {
                ProductName = foodItem.Name,
                PreviousQuantity = previousQuantity,
                UsedQuantity = usedQuantity,
                NewQuantity = foodItem.Quantity,
                Unit = foodItem.Unit
            });

            _logger.LogDebug(
                "Updating {ProductName}: {PreviousQuantity} - {UsedQuantity} = {NewQuantity} {Unit}",
                foodItem.Name, previousQuantity, usedQuantity, foodItem.Quantity, foodItem.Unit);
        }

        // Step 6: Persist all inventory changes to the database
        await _foodItemRepository.UpdateRangeAsync(itemsToUpdate, ct);

        _logger.LogInformation(
            "Successfully confirmed {RecipeCount} recipes and updated {InventoryCount} inventory items",
            recipes.Count, itemsToUpdate.Count);

        return new ConfirmRecipesResponseDto
        {
            Success = true,
            Message = $"Successfully confirmed {recipes.Count} recipe(s) and updated inventory.",
            ConfirmedRecipesCount = recipes.Count,
            InventoryUpdates = inventoryUpdates
        };
    }
}
