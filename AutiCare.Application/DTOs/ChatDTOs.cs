using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record StartChatRequest(int SpecialistId);

public class SendMessageRequest
{
    public int ChatId { get; set; }
    public string Content { get; set; } = string.Empty;
    
    // Optional. Defaults to "User". Use "ZoomLink" when sending a Zoom session message.
    
    public string MessageType { get; set; } = "User";
}


// Request body for POST /api/chat/send-zoom-link.
// Specialist sends confirmed session details + Zoom link to a parent chat.

public class SendZoomLinkRequest
{
    public int ChatId { get; set; }
    public string ZoomLink { get; set; } = string.Empty;
    /// <summary>Confirmed session date, e.g. "2026-06-01".</summary>
    public string ConfirmedDate { get; set; } = string.Empty;
    /// <summary>Confirmed session time, e.g. "14:00".</summary>
    public string ConfirmedTime { get; set; } = string.Empty;
    /// <summary>Optional note to the parent.</summary>
    public string? Note { get; set; }
}


/// Message response — includes MessageType so clients can render Zoom-link messages differently.
/// MessageType values: "User" | "ZoomLink" | "System"

public record MessageResponse(
    int MessageId,
    int ChatId,
    string Content,
    string SenderType,
    string SenderUserId,
    string MessageType,
    DateTime TimeStamp,
    bool IsRead
);

public record ChatResponse(
    int ChatId,
    int ParentId,
    string ParentName,
    int SpecialistId,
    string SpecialistName,
    DateTime LastMessageAt,
    string? LastMessage
);
