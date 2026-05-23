using System;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IProfileService
{
    Task UpdateProfileAsync(Guid userId, string role, UpdateProfileRequest request);
    Task UpdateProfilePictureAsync(Guid userId, string role, string profilePictureUrl);
    Task UpdateDoctorLicenseAsync(Guid userId, string licenseUrl);
}
