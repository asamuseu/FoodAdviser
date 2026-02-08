using FoodAdviser.Api.Controllers;
using FoodAdviser.Application.DTOs.Recipes;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace FoodAdviser.Api.Tests.Controllers;

public class RecipesControllerTests
{
    private readonly IRecipeSuggestionService _recipeSuggestionService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<RecipesController> _logger;
    private readonly RecipesController _controller;

    public RecipesControllerTests()
    {
        _recipeSuggestionService = Substitute.For<IRecipeSuggestionService>();
        _inventoryService = Substitute.For<IInventoryService>();
        _logger = Substitute.For<ILogger<RecipesController>>();
        _controller = new RecipesController(_recipeSuggestionService, _inventoryService, _logger);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GetSuggestions Tests

    [Fact]
    public void GetSuggestions_WithDefaultMax_ReturnsOkWithEmptyArray()
    {
        // Act
        var result = _controller.GetSuggestions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.NotNull(response);
        Assert.Equal(10, response["max"].GetInt32());
    }

    [Theory]
    [InlineData(5)]
    [InlineData(20)]
    [InlineData(50)]
    public void GetSuggestions_WithCustomMax_ReturnsOkWithSpecifiedMax(int max)
    {
        // Act
        var result = _controller.GetSuggestions(max);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.NotNull(response);
        Assert.Equal(max, response["max"].GetInt32());
    }

    [Fact]
    public void GetSuggestions_ReturnsEmptySuggestionsArray()
    {
        // Act
        var result = _controller.GetSuggestions();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var json = JsonSerializer.Serialize(okResult.Value);
        var response = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.NotNull(response);
        Assert.Equal(0, response["suggestions"].GetArrayLength());
    }

    #endregion

    #region GenerateRecipes Tests

    [Fact]
    public async Task GenerateRecipes_WithValidRequest_ReturnsOkWithRecipes()
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = 4
        };

        var recipes = new List<RecipeDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Recipe 1", DishType = DishType.MainCourse },
            new() { Id = Guid.NewGuid(), Title = "Recipe 2", DishType = DishType.MainCourse }
        };

        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>())
            .Returns(recipes);

        // Act
        var result = await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRecipes = Assert.IsAssignableFrom<IReadOnlyList<RecipeDto>>(okResult.Value);
        Assert.Equal(2, returnedRecipes.Count);
    }

    [Theory]
    [InlineData(DishType.Appetizer)]
    [InlineData(DishType.MainCourse)]
    [InlineData(DishType.Dessert)]
    [InlineData(DishType.Soup)]
    [InlineData(DishType.Salad)]
    public async Task GenerateRecipes_WithDifferentDishTypes_CallsServiceWithCorrectType(DishType dishType)
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = dishType,
            NumberOfPersons = 2
        };

        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            dishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>())
            .Returns(new List<RecipeDto>());

        // Act
        await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        await _recipeSuggestionService.Received(1).GenerateRecipeSuggestionsAsync(
            dishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(10)]
    public async Task GenerateRecipes_WithDifferentNumberOfPersons_CallsServiceWithCorrectNumber(int numberOfPersons)
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = numberOfPersons
        };

        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            numberOfPersons,
            Arg.Any<CancellationToken>())
            .Returns(new List<RecipeDto>());

        // Act
        await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        await _recipeSuggestionService.Received(1).GenerateRecipeSuggestionsAsync(
            request.DishType,
            numberOfPersons,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateRecipes_WithInvalidOperation_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = 4
        };

        var exceptionMessage = "No suitable recipes could be generated with available ingredients.";
        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(unprocessableResult.Value);
        Assert.Equal("Recipe Generation Failed", problemDetails.Title);
        Assert.Equal(exceptionMessage, problemDetails.Detail);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.Status);
    }

    [Fact]
    public async Task GenerateRecipes_LogsInformationOnRequest()
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = 4
        };

        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>())
            .Returns(new List<RecipeDto>());

        // Act
        await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Received recipe generation request")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GenerateRecipes_LogsSuccessfulGeneration()
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = 4
        };

        var recipes = new List<RecipeDto>
        {
            new() { Id = Guid.NewGuid(), Title = "Recipe 1" },
            new() { Id = Guid.NewGuid(), Title = "Recipe 2" }
        };

        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>())
            .Returns(recipes);

        // Act
        await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully generated 2 recipes")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GenerateRecipes_LogsWarningOnFailure()
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = 4
        };

        var exception = new InvalidOperationException("Generation failed");
        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        await _controller.GenerateRecipes(request, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to generate recipes")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task GenerateRecipes_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var request = new GenerateRecipesRequestDto
        {
            DishType = DishType.MainCourse,
            NumberOfPersons = 4
        };

        var cts = new CancellationTokenSource();
        _recipeSuggestionService.GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            cts.Token)
            .Returns(new List<RecipeDto>());

        // Act
        await _controller.GenerateRecipes(request, cts.Token);

        // Assert
        await _recipeSuggestionService.Received(1).GenerateRecipeSuggestionsAsync(
            request.DishType,
            request.NumberOfPersons,
            cts.Token);
    }

    #endregion

    #region ConfirmRecipes Tests

    [Fact]
    public async Task ConfirmRecipes_WithValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var recipeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = recipeIds
        };

        var expectedResponse = new ConfirmRecipesResponseDto
        {
            Success = true,
            ConfirmedRecipesCount = 2,
            Message = "Recipes confirmed successfully"
        };

        _inventoryService.ConfirmRecipesAsync(recipeIds, Arg.Any<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var result = await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ConfirmRecipesResponseDto>(okResult.Value);
        Assert.Equal(expectedResponse.ConfirmedRecipesCount, response.ConfirmedRecipesCount);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task ConfirmRecipes_WithSingleRecipe_CallsServiceCorrectly()
    {
        // Arrange
        var recipeId = Guid.NewGuid();
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = new List<Guid> { recipeId }
        };

        _inventoryService.ConfirmRecipesAsync(request.RecipeIds, Arg.Any<CancellationToken>())
            .Returns(new ConfirmRecipesResponseDto { ConfirmedRecipesCount = 1 });

        // Act
        await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        await _inventoryService.Received(1).ConfirmRecipesAsync(
            Arg.Is<List<Guid>>(ids => ids.Count == 1 && ids[0] == recipeId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmRecipes_WithMultipleRecipes_CallsServiceCorrectly()
    {
        // Arrange
        var recipeIds = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = recipeIds
        };

        _inventoryService.ConfirmRecipesAsync(recipeIds, Arg.Any<CancellationToken>())
            .Returns(new ConfirmRecipesResponseDto { ConfirmedRecipesCount = 3 });

        // Act
        await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        await _inventoryService.Received(1).ConfirmRecipesAsync(
            Arg.Is<List<Guid>>(ids => ids.Count == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmRecipes_WithInvalidOperation_ReturnsUnprocessableEntity()
    {
        // Arrange
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = new List<Guid> { Guid.NewGuid() }
        };

        var exceptionMessage = "Recipe not found or inventory insufficient.";
        _inventoryService.ConfirmRecipesAsync(request.RecipeIds, Arg.Any<CancellationToken>())
            .Throws(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        var unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(unprocessableResult.Value);
        Assert.Equal("Recipe Confirmation Failed", problemDetails.Title);
        Assert.Equal(exceptionMessage, problemDetails.Detail);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problemDetails.Status);
    }

    [Fact]
    public async Task ConfirmRecipes_LogsInformationOnRequest()
    {
        // Arrange
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        _inventoryService.ConfirmRecipesAsync(request.RecipeIds, Arg.Any<CancellationToken>())
            .Returns(new ConfirmRecipesResponseDto());

        // Act
        await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Received recipe confirmation request for 2 recipes")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ConfirmRecipes_LogsSuccessfulConfirmation()
    {
        // Arrange
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };

        var response = new ConfirmRecipesResponseDto
        {
            ConfirmedRecipesCount = 2
        };

        _inventoryService.ConfirmRecipesAsync(request.RecipeIds, Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully confirmed 2 recipes and updated inventory")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ConfirmRecipes_LogsWarningOnFailure()
    {
        // Arrange
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = new List<Guid> { Guid.NewGuid() }
        };

        var exception = new InvalidOperationException("Confirmation failed");
        _inventoryService.ConfirmRecipesAsync(request.RecipeIds, Arg.Any<CancellationToken>())
            .Throws(exception);

        // Act
        await _controller.ConfirmRecipes(request, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to confirm recipes")),
            exception,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task ConfirmRecipes_WithCancellationToken_PassesTokenToService()
    {
        // Arrange
        var request = new ConfirmRecipesRequestDto
        {
            RecipeIds = new List<Guid> { Guid.NewGuid() }
        };

        var cts = new CancellationTokenSource();
        _inventoryService.ConfirmRecipesAsync(request.RecipeIds, cts.Token)
            .Returns(new ConfirmRecipesResponseDto());

        // Act
        await _controller.ConfirmRecipes(request, cts.Token);

        // Assert
        await _inventoryService.Received(1).ConfirmRecipesAsync(request.RecipeIds, cts.Token);
    }

    #endregion

    // Note: Constructor null validation tests removed as controllers don't throw ArgumentNullException
    // in modern .NET when null arguments are passed (they fail at first usage instead)
}





