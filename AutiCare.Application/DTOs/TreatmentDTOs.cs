using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record CreateTreatmentPlanRequest(
    int ChildId,
    int SpecialistId,
    DateTime StartDate,
    DateTime? EndDate,
    string? Goal,
    string? Notes
);

public record UpdateTreatmentPlanRequest(
    string? Goal,
    string? Notes,
    string? Progress,
    DateTime? EndDate
);

public record TreatmentPlanResponse(
    int TreatmentId,
    int ChildId,
    string ChildName,
    int SpecialistId,
    string SpecialistName,
    string? Goal,
    string? Notes,
    string? Progress,
    DateTime StartDate,
    DateTime? EndDate,
    int TotalSessions,
    DateTime CreatedAt
);
