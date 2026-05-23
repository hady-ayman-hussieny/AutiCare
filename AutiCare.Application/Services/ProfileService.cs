using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class ProfileService : IProfileService
{
    private readonly IParentRepository _parentRepo;
    private readonly IDoctorRepository _specialistRepo;

    public ProfileService(IParentRepository parentRepo, IDoctorRepository specialistRepo)
    {
        _parentRepo = parentRepo;
        _specialistRepo = specialistRepo;
    }

    public async Task UpdateProfileAsync(Guid userId, string role, UpdateProfileRequest request)
    {
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Parent not found");
            if (request.Name != null) parent.Name = request.Name;
            if (request.Phone != null) parent.Phone = request.Phone;
            if (request.Address != null) parent.Address = request.Address;
            _parentRepo.Update(parent);
            await _parentRepo.SaveChangesAsync();
        }
        else if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _specialistRepo.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Specialist not found");
            if (request.Name != null) specialist.Name = request.Name;
            if (request.Phone != null) specialist.Phone = request.Phone;
            if (request.Address != null) specialist.Address = request.Address;
            if (request.Bio != null) specialist.Bio = request.Bio;
            _specialistRepo.Update(specialist);
            await _specialistRepo.SaveChangesAsync();
        }
    }

    public async Task UpdateProfilePictureAsync(Guid userId, string role, string profilePictureUrl)
    {
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Parent not found");
            parent.ProfilePictureJson = profilePictureUrl;
            _parentRepo.Update(parent);
            await _parentRepo.SaveChangesAsync();
        }
        else if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _specialistRepo.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Specialist not found");
            specialist.ProfilePictureJson = profilePictureUrl;
            _specialistRepo.Update(specialist);
            await _specialistRepo.SaveChangesAsync();
        }
    }

    public async Task UpdateDoctorLicenseAsync(Guid userId, string licenseUrl)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(userId) ?? throw new KeyNotFoundException("Specialist not found");
        specialist.LicenseNumber = licenseUrl; // Using this to store URL or we can use another field if it existed
        _specialistRepo.Update(specialist);
        await _specialistRepo.SaveChangesAsync();
    }
}
