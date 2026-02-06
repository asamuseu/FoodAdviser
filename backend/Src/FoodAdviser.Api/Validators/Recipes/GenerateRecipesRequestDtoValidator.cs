using FluentValidation;
using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Domain.Enums;

namespace FoodAdviser.Api.Validators.Recipes;

/// <summary>
/// Validator for <see cref="GenerateRecipesRequestDto"/>.
/// </summary>
public sealed class GenerateRecipesRequestDtoValidator : AbstractValidator<GenerateRecipesRequestDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenerateRecipesRequestDtoValidator"/> class.
    /// </summary>
    public GenerateRecipesRequestDtoValidator()
    {
        RuleFor(x => x.DishType)
            .NotEqual(DishType.Undefined)
            .WithMessage("A valid dish type must be specified.")
            .IsInEnum()
            .WithMessage("Invalid dish type value.");

        RuleFor(x => x.NumberOfPersons)
            .GreaterThan(0)
            .WithMessage("Number of persons must be at least 1.")
            .LessThanOrEqualTo(100)
            .WithMessage("Number of persons cannot exceed 100.");
    }
}
