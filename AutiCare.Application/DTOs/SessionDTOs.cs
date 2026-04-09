using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record CreateSessionRequest(
    int TreatmentId,
    DateTime SessionDate,
    TimeSpan? SessionTime,
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
    int TreatmentId,
    DateTime SessionDate,
    TimeSpan? SessionTime,
    string? SessionNotes,
    string? ActivityNotes,
    string? Report,
    DateTime CreatedAt
);
