using FoodAdviser.Api.Controllers;
using FoodAdviser.Application.DTOs.Inventory;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NSubstitute;

namespace FoodAdviser.Api.Tests.Controllers;

public class InventoryControllerTests
{
    private readonly IFoodItemRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly InventoryController _controller;
    private readonly Guid _userId;

    public InventoryControllerTests()
    {
        _repository = Substitute.For<IFoodItemRepository>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _userId = Guid.NewGuid();
        _currentUserService.GetRequiredUserId().Returns(_userId);
        
        _controller = new InventoryController(_repository, _currentUserService);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        
        // Setup ProblemDetailsFactory
        var problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        problemDetailsFactory.CreateProblemDetails(
            Arg.Any<HttpContext>(),
            Arg.Any<int?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>()).Returns(callInfo => new ProblemDetails
            {
                Status = callInfo.ArgAt<int?>(1),
                Detail = callInfo.ArgAt<string?>(4)
            });
        _controller.ProblemDetailsFactory = problemDetailsFactory;
    }

    #region Get Tests

    [Fact]
    public async Task Get_WithValidParameters_ReturnsOkWithFoodItems()
    {
        // Arrange
        var items = new List<FoodItem>
        {
            new() { Id = Guid.NewGuid(), Name = "Milk", Quantity = 1, Unit = "L", UserId = _userId },
            new() { Id = Guid.NewGuid(), Name = "Eggs", Quantity = 12, Unit = "pcs", UserId = _userId }
        };

        _repository.GetPagedAsync(1, 20, _userId, Arg.Any<CancellationToken>())
            .Returns(items);

        // Act
        var result = await _controller.Get(1, 20);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<FoodItemDto>>(okResult.Value);
        Assert.Equal(2, returnedItems.Count());
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task Get_WithInvalidParameters_ReturnsBadRequest(int page, int pageSize)
    {
        // Act
        var result = await _controller.Get(page, pageSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Contains("must be positive", problemDetails.Detail!);
    }

    [Fact]
    public async Task Get_WithDefaultParameters_UsesPageOneAndTwentyItems()
    {
        // Arrange
        _repository.GetPagedAsync(1, 20, _userId, Arg.Any<CancellationToken>())
            .Returns(new List<FoodItem>());

        // Act
        var result = await _controller.Get();

        // Assert
        await _repository.Received(1).GetPagedAsync(1, 20, _userId, Arg.Any<CancellationToken>());
        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Get_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _repository.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<Guid>(), cts.Token)
            .Returns(new List<FoodItem>());

        // Act
        await _controller.Get(1, 20, cts.Token);

        // Assert
        await _repository.Received(1).GetPagedAsync(1, 20, _userId, cts.Token);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithExistingItem_ReturnsOkWithItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var item = new FoodItem
        {
            Id = itemId,
            Name = "Milk",
            Quantity = 1,
            Unit = "L",
            UserId = _userId
        };

        _repository.GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns(item);

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItem = Assert.IsType<FoodItemDto>(okResult.Value);
        Assert.Equal(itemId, returnedItem.Id);
        Assert.Equal("Milk", returnedItem.Name);
    }

    [Fact]
    public async Task GetById_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _repository.GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns((FoodItem?)null);

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_WithDifferentUserId_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        
        // Item exists but belongs to another user - repository returns null
        _repository.GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns((FoodItem?)null);

        // Act
        var result = await _controller.GetById(itemId, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidDto_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateFoodItemDto
        {
            Name = "Milk",
            Quantity = 1,
            Unit = "L"
        };

        var createdItem = new FoodItem
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Quantity = createDto.Quantity,
            Unit = createDto.Unit,
            UserId = _userId
        };

        _repository.AddAsync(Arg.Any<FoodItem>(), Arg.Any<CancellationToken>())
            .Returns(createdItem);

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(InventoryController.GetById), createdResult.ActionName);
        var returnedItem = Assert.IsType<FoodItemDto>(createdResult.Value);
        Assert.Equal(createdItem.Id, returnedItem.Id);
        Assert.Equal(createDto.Name, returnedItem.Name);
    }

    [Fact]
    public async Task Create_WithInvalidModelState_ReturnsValidationProblem()
    {
        // Arrange
        var createDto = new CreateFoodItemDto
        {
            Name = "",
            Quantity = -1,
            Unit = ""
        };

        _controller.ModelState.AddModelError("Name", "Name is required");
        _controller.ModelState.AddModelError("Quantity", "Quantity must be positive");

        // Act
        var result = await _controller.Create(createDto, CancellationToken.None);

        // Assert
        var validationResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, validationResult.StatusCode);
    }

    [Fact]
    public async Task Create_AssignsCurrentUserId()
    {
        // Arrange
        var createDto = new CreateFoodItemDto
        {
            Name = "Milk",
            Quantity = 1,
            Unit = "L"
        };

        FoodItem? capturedItem = null;
        _repository.AddAsync(Arg.Do<FoodItem>(item => capturedItem = item), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<FoodItem>());

        // Act
        await _controller.Create(createDto, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedItem);
        Assert.Equal(_userId, capturedItem.UserId);
        Assert.NotEqual(Guid.Empty, capturedItem.Id);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithMatchingIdAndExistingItem_ReturnsNoContent()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var updateDto = new FoodItemDto
        {
            Id = itemId,
            Name = "Updated Milk",
            Quantity = 2,
            Unit = "L"
        };

        var existingItem = new FoodItem
        {
            Id = itemId,
            Name = "Milk",
            Quantity = 1,
            Unit = "L",
            UserId = _userId
        };

        _repository.GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns(existingItem);

        // Act
        var result = await _controller.Update(itemId, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _repository.Received(1).UpdateAsync(Arg.Any<FoodItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_WithMismatchedId_ReturnsBadRequest()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var differentId = Guid.NewGuid();
        var updateDto = new FoodItemDto
        {
            Id = differentId,
            Name = "Milk",
            Quantity = 1,
            Unit = "L"
        };

        // Act
        var result = await _controller.Update(itemId, updateDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequestResult.Value);
        Assert.Contains("mismatch", problemDetails.Detail!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Update_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var updateDto = new FoodItemDto
        {
            Id = itemId,
            Name = "Milk",
            Quantity = 1,
            Unit = "L"
        };

        _repository.GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns((FoodItem?)null);

        // Act
        var result = await _controller.Update(itemId, updateDto, CancellationToken.None);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<FoodItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Update_VerifiesItemBelongsToUser()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var updateDto = new FoodItemDto
        {
            Id = itemId,
            Name = "Milk",
            Quantity = 1,
            Unit = "L"
        };

        _repository.GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns((FoodItem?)null);

        // Act
        var result = await _controller.Update(itemId, updateDto, CancellationToken.None);

        // Assert
        await _repository.Received(1).GetByIdAsync(itemId, _userId, Arg.Any<CancellationToken>());
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithExistingItem_ReturnsNoContent()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        _repository.DeleteAsync(itemId, _userId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.Delete(itemId, CancellationToken.None);

        // Assert
        Assert.IsType<NoContentResult>(result);
        await _repository.Received(1).DeleteAsync(itemId, _userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_PassesUserIdToRepository()
    {
        // Arrange
        var itemId = Guid.NewGuid();

        // Act
        await _controller.Delete(itemId, CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(itemId, _userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        // Act
        await _controller.Delete(itemId, cts.Token);

        // Assert
        await _repository.Received(1).DeleteAsync(itemId, _userId, cts.Token);
    }

    #endregion
}



