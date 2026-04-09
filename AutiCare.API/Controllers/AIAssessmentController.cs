using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/assessment")]
[Authorize]
public class AIAssessmentController : BaseController
{
    private readonly IAiAssessmentService _aiService;
    public AIAssessmentController(IAiAssessmentService aiService) => _aiService = aiService;

    [HttpGet("questions/{testId}")]
    public async Task<IActionResult> GetQuestions(int testId)
    {
        var questions = await _aiService.GetQuestionsAsync(testId);
        return Ok(questions);
    }

    [HttpPost("start")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> StartTest(StartTestRequest request)
    {
        var id = await _aiService.StartTestAsync(GetUserId(), request);
        return Ok(new { ParentTestId = id });
    }

    [HttpPost("submit")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> SubmitAnswers(SubmitAnswersRequest request)
    {
        var result = await _aiService.SubmitAnswersAsync(GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("results/{childId}")]
    public async Task<IActionResult> GetResults(int childId)
    {
        var results = await _aiService.GetChildResultsAsync(childId, GetUserId(), GetUserRole());
        return Ok(results);
    }

    [HttpGet("analytics/{childId}")]
    public async Task<IActionResult> GetAnalytics(int childId)
    {
        var analytics = await _aiService.GetAnalyticsAsync(childId, GetUserId(), GetUserRole());
        return Ok(analytics);
    }
}
