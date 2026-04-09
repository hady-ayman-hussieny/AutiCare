using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/dashboard")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("parent")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetParentDashboard()
    {
        var data = await _dashboardService.GetParentDashboardAsync(GetUserId());
        return Ok(data);
    }

    [HttpGet("specialist")]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> GetSpecialistDashboard()
    {
        var data = await _dashboardService.GetSpecialistDashboardAsync(GetUserId());
        return Ok(data);
    }
}
