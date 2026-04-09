using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/treatment-plans")]
[Authorize]
public class TreatmentPlanController : BaseController
{
    private readonly ITreatmentService _treatmentService;
    public TreatmentPlanController(ITreatmentService treatmentService) =>
        _treatmentService = treatmentService;

    [HttpPost]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> CreatePlan(CreateTreatmentPlanRequest request)
    {
        var plan = await _treatmentService.CreatePlanAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetPlan), new { id = plan.TreatmentId }, plan);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlan(int id)
    {
        var plan = await _treatmentService.GetPlanAsync(id,GetUserId());
        return plan == null ? NotFound() : Ok(plan);
    }

    [HttpGet("child/{childId}")]
    public async Task<IActionResult> GetByChild(int childId)
    {
        var plans = await _treatmentService.GetByChildAsync(childId, GetUserId(), GetUserRole());
        return Ok(plans);
    }

    [HttpGet("my-plans")]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> GetMyPlans()
    {
        var plans = await _treatmentService.GetMyPlansAsync(GetUserId());
        return Ok(plans);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> UpdatePlan(int id, UpdateTreatmentPlanRequest request)
    {
        await _treatmentService.UpdateAsync(id, request, GetUserId());
        return Ok(new { message = "Updated" });
    }
}
