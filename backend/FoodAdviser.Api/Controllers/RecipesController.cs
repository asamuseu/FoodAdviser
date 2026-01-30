using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodAdviser.Api.Controllers;

/// <summary>
/// Provides recipe suggestions based on available inventory.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeSuggestionService _recipeSuggestionService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<RecipesController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipesController"/> class.
    /// </summary>
    /// <param name="recipeSuggestionService">The recipe suggestion service.</param>
    /// <param name="inventoryService">The inventory service.</param>
    /// <param name="logger">The logger instance.</param>
    public RecipesController(
        IRecipeSuggestionService recipeSuggestionService,
        IInventoryService inventoryService,
        ILogger<RecipesController> logger)
    {
        _recipeSuggestionService = recipeSuggestionService;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>Gets recipe suggestions.</summary>
    [HttpGet("suggestions")]
    public IActionResult GetSuggestions([FromQuery] int max = 10)
    {
        return Ok(new { suggestions = Array.Empty<object>(), max });
    }

    /// <summary>
    /// Generates recipe suggestions based on available inventory for the specified dish type and number of persons.
    /// </summary>
    /// <param name="request">The request containing dish type and number of persons.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of generated recipes.</returns>
    /// <response code="200">Returns the generated recipes.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="422">If no suitable recipes could be generated with available ingredients.</response>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(IReadOnlyList<RecipeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<IReadOnlyList<RecipeDto>>> GenerateRecipes(
        [FromBody] GenerateRecipesRequestDto request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Received recipe generation request: DishType={DishType}, NumberOfPersons={NumberOfPersons}",
            request.DishType, request.NumberOfPersons);

        try
        {
            var recipes = await _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
                request.DishType,
                request.NumberOfPersons,
                ct);

            _logger.LogInformation("Successfully generated {Count} recipes", recipes.Count);
            return Ok(recipes);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to generate recipes");
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Recipe Generation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
                Instance = HttpContext.Request.Path
            });
        }
    }

    /// <summary>
    /// Confirms the preparation of specified recipes and updates the inventory accordingly.
    /// </summary>
    /// <param name="request">The request containing recipe IDs to confirm.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A response containing confirmation details and inventory updates.</returns>
    /// <response code="200">Returns the confirmation result with inventory updates.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="422">If recipes are not found or inventory is insufficient.</response>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ConfirmRecipesResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ConfirmRecipesResponseDto>> ConfirmRecipes(
        [FromBody] ConfirmRecipesRequestDto request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Received recipe confirmation request for {Count} recipes",
            request.RecipeIds.Count);

        try
        {
            var result = await _inventoryService.ConfirmRecipesAsync(request.RecipeIds, ct);

            _logger.LogInformation(
                "Successfully confirmed {Count} recipes and updated inventory",
                result.ConfirmedRecipesCount);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to confirm recipes");
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Recipe Confirmation Failed",
                Detail = ex.Message,
                Status = StatusCodes.Status422UnprocessableEntity,
                Instance = HttpContext.Request.Path
            });
        }
    }
}
