using System;

namespace AutiCare.Application.DTOs;

public record ParentDashboardResponse(
    int TotalChildren,
    int UpcomingBookings,
    int CompletedTests,
    int UnreadNotifications
);

public record SpecialistDashboardResponse(
    int TotalPatients,
    int UpcomingSessionsCount,
    int PendingMessagesCount,
    int ReportsToReviewCount,
    System.Collections.Generic.List<UpcomingSessionDto> UpcomingSessions,
    System.Collections.Generic.List<LatestNoteDto> LatestNotes,
    System.Collections.Generic.List<PatientCardDto> PatientCards
);

public record UpcomingSessionDto(
    string ChildName,
    DateTime SessionDate,
    int? Duration,
    string? MeetingLink
);

public record LatestNoteDto(
    string? TherapistNotes,
    string? ParentNotes
);

public record PatientCardDto(
    string ChildName,
    int Age,
    string ASDLevel,
    string AssignedTherapist,
    int GoalPercentage
);
