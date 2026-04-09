using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record CreateChildRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    string? MedicalHistory
);

public record UpdateChildRequest(
    string? FirstName,
    string? LastName,
    string? MedicalHistory
);

public record ChildResponse(
    int ChildId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    int AgeInYears,
    string? MedicalHistory,
    DateTime CreatedAt
);
