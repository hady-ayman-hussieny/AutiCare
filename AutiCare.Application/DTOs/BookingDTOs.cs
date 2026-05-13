using System;
using System.Text.Json.Serialization;

namespace AutiCare.Application.DTOs;

public record CreateBookingRequest(
    int SpecialistId,
    int? ChildId,
    [property: JsonPropertyName("bookingDate")] DateTime PreferredDate,
    [property: JsonPropertyName("bookingTime")] TimeSpan? PreferredTime,
    string? Reason
);

public record UpdateBookingRequest(
    [property: JsonPropertyName("bookingDate")] DateTime? PreferredDate,
    [property: JsonPropertyName("bookingTime")] TimeSpan? PreferredTime,
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
    [property: JsonPropertyName("bookingDate")] DateTime PreferredDate,
    [property: JsonPropertyName("bookingTime")] TimeSpan? PreferredTime,
    string Status,
    string? Reason,
    DateTime CreatedAt
);
