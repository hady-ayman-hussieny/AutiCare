using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/specialists")]
[Authorize]
public class SpecialistsController : BaseController
{
    private readonly IDoctorRepository _doctorRepo;
    public SpecialistsController(IDoctorRepository doctorRepo) => _doctorRepo = doctorRepo;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationFilter filter)
    {
        var (specialists, total) = await _doctorRepo.GetDoctorsAsync(filter);
        var responses = specialists.Select(s => new SpecialistResponse(
            s.SpecialistId, s.Name, s.Email,
            s.Specialization, s.YearsExperience, s.Bio, s.LicenseNumber));

        return Ok(new PagedResponse<SpecialistResponse>(responses, filter.PageNumber, filter.PageSize, total));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await _doctorRepo.GetByIdAsync(id);
        if (s == null) return NotFound();
        return Ok(new SpecialistResponse(s.SpecialistId, s.Name, s.Email,
            s.Specialization, s.YearsExperience, s.Bio, s.LicenseNumber));
    }
}
