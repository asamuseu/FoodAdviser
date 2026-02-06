using AutoMapper;
using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Domain.Entities;

namespace FoodAdviser.Application.Mapping;

/// <summary>
/// AutoMapper profile for Recipe entity and DTOs.
/// </summary>
public class RecipeProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeProfile"/> class.
    /// </summary>
    public RecipeProfile()
    {
        CreateMap<Recipe, RecipeDto>();
        CreateMap<Ingredient, IngredientDto>();
        CreateMap<IngredientDto, Ingredient>();
        CreateMap<RecipeDto, Recipe>();
    }
}
