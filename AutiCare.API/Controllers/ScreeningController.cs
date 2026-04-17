using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AutiCare.API.Controllers;

[Route("api/screening")]
[ApiController]
[Authorize]
public class ScreeningController : BaseController
{
    private readonly IScreeningService _screeningService;

    public ScreeningController(IScreeningService screeningService)
    {
        _screeningService = screeningService;
    }

    /// <summary>
    /// Submits a screening assessment with exactly 10 questions and retrieves the AI prediction.
    /// </summary>
    [HttpPost("submit")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(SubmitScreeningResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> SubmitScreening([FromBody] SubmitScreeningRequest request)
    {
        try
        {
            var result = await _screeningService.SubmitScreeningAsync(request);
            return Ok(result);
        }
        catch (System.ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all past screening prediction results for a specific child.
    /// </summary>
    [HttpGet("results/{childId}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(System.Collections.Generic.IEnumerable<ScreeningResultResponse>), 200)]
    public async Task<IActionResult> GetResults(int childId)
    {
        try 
        {
            var results = await _screeningService.GetResultsByChildIdAsync(childId);
            return Ok(results);
        }
        catch (System.Collections.Generic.KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
