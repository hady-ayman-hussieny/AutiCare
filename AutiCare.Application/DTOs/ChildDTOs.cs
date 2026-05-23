using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record CreateChildRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    bool FamilyAutismHistory,
    bool JaundiceHistory,
    string? MedicalHistory
);

public record UpdateChildRequest(
    string? FirstName,
    string? LastName,
    bool? FamilyAutismHistory,
    bool? JaundiceHistory,
    string? MedicalHistory
);

public record ChildResponse(
    int ChildId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Gender,
    int AgeInYears,
    bool FamilyAutismHistory,
    bool JaundiceHistory,
    string? MedicalHistory,
    DateTime CreatedAt
);
