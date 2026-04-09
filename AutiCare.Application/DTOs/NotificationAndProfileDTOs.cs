using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record NotificationResponse(
    int NotificationId,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);

public record UpdateProfileRequest(
    string? Name,
    string? Phone,
    string? Address,
    string? Bio
);
