using FoodAdviser.Application.DTOs.Inventory;
using FoodAdviser.Application.Services.Interfaces;
using FoodAdviser.Domain.Repositories;
using FoodAdviser.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodAdviser.Api.Controllers;

/// <summary>
/// Manages food inventory items.
/// All operations are scoped to the authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IFoodItemRepository _repo;
    private readonly ICurrentUserService _currentUserService;
    
    public InventoryController(IFoodItemRepository repo, ICurrentUserService currentUserService)
    {
        _repo = repo;
        _currentUserService = currentUserService;
    }

    /// <summary>Gets a paginated list of food items.</summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Items per page.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FoodItemDto>>> Get(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest, detail: "page and pageSize must be positive"));
        
        var userId = _currentUserService.GetRequiredUserId();
        var items = await _repo.GetPagedAsync(page, pageSize, userId, ct);
        var result = items.Select(item => item.ToDto()).ToList();
        return Ok(result);
    }

    /// <summary>Gets a food item by id.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FoodItemDto>> GetById(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.GetRequiredUserId();
        var item = await _repo.GetByIdAsync(id, userId, ct);
        if (item is null) return NotFound();
        return Ok(item.ToDto());
    }

    /// <summary>Creates a new food item.</summary>
    [HttpPost]
    public async Task<ActionResult<FoodItemDto>> Create([FromBody] CreateFoodItemDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        
        var userId = _currentUserService.GetRequiredUserId();
        var entity = dto.ToEntity(userId);
        entity.Id = Guid.NewGuid();
        var created = await _repo.AddAsync(entity, ct);
        var result = created.ToDto();
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Updates an existing food item.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] FoodItemDto dto, CancellationToken ct)
    {
        if (id != dto.Id) return BadRequest(ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status400BadRequest, detail: "Id mismatch"));
        
        var userId = _currentUserService.GetRequiredUserId();
        
        // Verify the item belongs to the user
        var existing = await _repo.GetByIdAsync(id, userId, ct);
        if (existing is null) return NotFound();
        
        var entity = dto.ToEntity(userId);
        await _repo.UpdateAsync(entity, ct);
        return NoContent();
    }

    /// <summary>Deletes a food item.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.GetRequiredUserId();
        await _repo.DeleteAsync(id, userId, ct);
        return NoContent();
    }
}
