using FluentValidation;
using FoodAdviser.Application.DTOs.Recipes;

namespace FoodAdviser.Api.DTOs.Recipes.Validators;

/// <summary>
/// Validator for <see cref="ConfirmRecipesRequestDto"/>.
/// </summary>
public sealed class ConfirmRecipesRequestDtoValidator : AbstractValidator<ConfirmRecipesRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfirmRecipesRequestDtoValidator"/> class.
    /// </summary>
    public ConfirmRecipesRequestDtoValidator()
    {
        RuleFor(x => x.RecipeIds)
            .NotNull()
            .WithMessage("Recipe IDs collection is required.")
            .NotEmpty()
            .WithMessage("At least one recipe ID must be provided.");

        RuleForEach(x => x.RecipeIds)
            .NotEmpty()
            .WithMessage("Recipe ID cannot be empty.");
    }
}
