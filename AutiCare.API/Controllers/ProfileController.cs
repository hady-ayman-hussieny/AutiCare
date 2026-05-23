using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/profile")]
[Authorize]
public class ProfileController : BaseController
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        await _profileService.UpdateProfileAsync(GetUserId(), GetUserRole(), request);
        return Ok(new { message = "Profile updated successfully." });
    }

    [HttpPut("picture")]
    public async Task<IActionResult> UpdateProfilePicture([FromBody] string profilePictureUrl)
    {
        await _profileService.UpdateProfilePictureAsync(GetUserId(), GetUserRole(), profilePictureUrl);
        return Ok(new { message = "Profile picture updated successfully." });
    }

    [HttpPut("license")]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> UpdateDoctorLicense([FromBody] string licenseUrl)
    {
        await _profileService.UpdateDoctorLicenseAsync(GetUserId(), licenseUrl);
        return Ok(new { message = "License updated successfully." });
    }
}
