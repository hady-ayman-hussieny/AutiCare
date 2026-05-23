using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

/// <summary>
/// Official Autism Screening module — AI-powered developmental screening.
/// </summary>
[Route("api/screening")]
[ApiController]
[Authorize]
[Tags("Screening")]
public class ScreeningController : BaseController
{
    private readonly IScreeningService _screeningService;

    public ScreeningController(IScreeningService screeningService)
    {
        _screeningService = screeningService;
    }

    /// <summary>
    /// Starts a new screening session for a child.
    /// </summary>
    [HttpPost("start")]
    [Authorize(Roles = "Parent")]
    [ProducesResponseType(typeof(StartScreeningResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> StartScreening([FromBody] StartScreeningRequest request)
    {
        try
        {
            var result = await _screeningService.StartScreeningAsync(request.ChildId, GetUserId());
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns the ordered list of screening questions.
    /// </summary>
    [HttpGet("questions")]
    [ProducesResponseType(typeof(IReadOnlyList<ScreeningQuestionResponse>), 200)]
    public IActionResult GetQuestions()
    {
        var questions = _screeningService.GetQuestions();
        return Ok(questions);
    }

    /// <summary>
    /// Submits screening answers and retrieves the AI prediction.
    /// </summary>
    [HttpPost("submit")]
    [Authorize(Roles = "Parent")]
    [ProducesResponseType(typeof(SubmitScreeningResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> SubmitScreening([FromBody] SubmitScreeningRequest request)
    {
        try
        {
            var result = await _screeningService.SubmitScreeningAsync(request, GetUserId());
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(503, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all past screening results for a specific child (newest first).
    /// </summary>
    [HttpGet("results/{childId}")]
    [ProducesResponseType(typeof(IEnumerable<ScreeningResultResponse>), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetResults(int childId)
    {
        try
        {
            var results = await _screeningService.GetResultsByChildIdAsync(childId, GetUserId(), GetUserRole());
            return Ok(results);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets screening analytics summary for a specific child.
    /// </summary>
    [HttpGet("analytics/{childId}")]
    [ProducesResponseType(typeof(ScreeningAnalyticsResponse), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAnalytics(int childId)
    {
        try
        {
            var analytics = await _screeningService.GetAnalyticsAsync(childId, GetUserId(), GetUserRole());
            return Ok(analytics);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { error = ex.Message });
        }
    }
}
