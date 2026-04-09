using System;

namespace AutiCare.Application.DTOs;

public record CreateBookingRequest(
    int SpecialistId,
    int? ChildId,
    DateTime BookingDate,
    TimeSpan? BookingTime,
    string? Reason
);

public record UpdateBookingRequest(
    DateTime? BookingDate,
    TimeSpan? BookingTime,
    string? Status,
    string? Reason
);

public record BookingResponse(
    int BookingId,
    int ParentId,
    string ParentName,
    int SpecialistId,
    string SpecialistName,
    int? ChildId,
    string? ChildName,
    DateTime BookingDate,
    TimeSpan? BookingTime,
    string Status,
    string? Reason,
    DateTime CreatedAt
);
