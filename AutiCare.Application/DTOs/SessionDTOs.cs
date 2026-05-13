using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record CreateSessionRequest(
    int? TreatmentId,
    int ParentId,
    int SpecialistId,
    DateTime SessionDate,
    TimeSpan? SessionTime,
    int? Duration,
    string? MeetingLink,
    string? SessionNotes,
    string? ActivityNotes,
    string? Report
);

public record UpdateSessionRequest(
    string? SessionNotes,
    string? ActivityNotes,
    string? Report
);

public record SessionResponse(
    int SessionId,
    int? TreatmentId,
    int ParentId,
    int SpecialistId,
    DateTime SessionDate,
    TimeSpan? SessionTime,
    int? Duration,
    string? MeetingLink,
    string? SessionNotes,
    string? ActivityNotes,
    string? Report,
    DateTime CreatedAt
);
