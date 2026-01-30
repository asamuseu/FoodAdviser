namespace FoodAdviser.Application.DTOs.Recipes;

/// <summary>
/// Request DTO for confirming recipe preparation and updating inventory.
/// </summary>
public class ConfirmRecipesRequestDto
{
    /// <summary>
    /// Collection of recipe IDs to confirm preparation for.
    /// </summary>
    public IReadOnlyList<Guid> RecipeIds { get; set; } = new List<Guid>();
}
