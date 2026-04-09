using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record StartChatRequest(int SpecialistId);

public record SendMessageRequest(string Content);

public record MessageResponse(
    int MessageId,
    int ChatId,
    string Content,
    string SenderType,
    string SenderUserId,
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
