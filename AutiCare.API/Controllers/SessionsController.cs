using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/sessions")]
[Authorize]
public class SessionsController : BaseController
{
    private readonly ITreatmentService _treatmentService;
    public SessionsController(ITreatmentService treatmentService) =>
        _treatmentService = treatmentService;

    [HttpPost]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> AddSession(CreateSessionRequest request)
    {
        var session = await _treatmentService.AddSessionAsync(GetUserId(), request);
        return Ok(session);
    }

    [HttpGet("treatment/{treatmentId}")]
    public async Task<IActionResult> GetSessions(int treatmentId)
    {        
        var sessions = await _treatmentService.GetSessionsAsync(GetUserId(), GetUserRole(), treatmentId);
        return Ok(sessions);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> UpdateSession(int id, UpdateSessionRequest request)
    {
        await _treatmentService.UpdateSessionAsync(id, request, GetUserId());
        return Ok(new { message = "Updated" });
    }
}
