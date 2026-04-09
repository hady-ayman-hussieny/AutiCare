using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record SpecialistResponse(
    int SpecialistId,
    string Name,
    string? Email,
    string? Specialization,
    int YearsExperience,
    string? Bio,
    string? LicenseNumber
);
