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
    int UpcomingBookings,
    int ActiveTreatmentPlans,
    int UnreadNotifications
);
