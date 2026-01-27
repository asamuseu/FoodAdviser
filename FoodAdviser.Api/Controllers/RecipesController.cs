using Microsoft.AspNetCore.Mvc;

namespace FoodAdviser.Api.Controllers;

/// <summary>
/// Provides recipe suggestions based on available inventory.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    /// <summary>Gets recipe suggestions.</summary>
    [HttpGet("suggestions")]
    public IActionResult GetSuggestions([FromQuery] int max = 10)
    {
        return Ok(new { suggestions = Array.Empty<object>(), max });
    }
}
