using FoodAdviser.Application.DTOs.Inventory;
using FoodAdviser.Domain.Entities;
using FoodAdviser.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FoodAdviser.Api.Controllers;

/// <summary>
/// Manages food inventory items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IFoodItemRepository _repo;
    public InventoryController(IFoodItemRepository repo)
    {
        _repo = repo;
    }

    /// <summary>Gets a paginated list of food items.</summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Items per page.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoodItemDto>>> Get(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest, detail: "page and pageSize must be positive"));
        var items = await _repo.GetPagedAsync(page, pageSize, ct);
        var result = items.Select(MapToDto).ToList();
        return Ok(result);
    }

    /// <summary>Gets a food item by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FoodItemDto>> GetById(Guid id, CancellationToken ct)
    {
        var item = await _repo.GetByIdAsync(id, ct);
        if (item is null) return NotFound();
        return Ok(MapToDto(item));
    }

    /// <summary>Creates a new food item.</summary>
    [HttpPost]
    public async Task<ActionResult<FoodItemDto>> Create([FromBody] CreateFoodItemDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var entity = MapFromCreate(dto);
        entity.Id = Guid.NewGuid();
        var created = await _repo.AddAsync(entity, ct);
        var result = MapToDto(created);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates an existing food item.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FoodItemDto dto, CancellationToken ct)
    {
        if (id != dto.Id) return BadRequest(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest, detail: "Id mismatch"));
        var entity = MapFromDto(dto);
        await _repo.UpdateAsync(entity, ct);
        return NoContent();
    }

    /// <summary>Deletes a food item.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _repo.DeleteAsync(id, ct);
        return NoContent();
    }

    private static FoodItemDto MapToDto(FoodItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Quantity = item.Quantity,
        Unit = item.Unit,
        ExpiresAt = item.ExpiresAt
    };

    private static FoodItem MapFromDto(FoodItemDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        Quantity = dto.Quantity,
        Unit = dto.Unit,
        ExpiresAt = dto.ExpiresAt
    };

    private static FoodItem MapFromCreate(CreateFoodItemDto dto) => new()
    {
        Name = dto.Name,
        Quantity = dto.Quantity,
        Unit = dto.Unit,
        ExpiresAt = dto.ExpiresAt
    };
}
