using System;

namespace AutiCare.Application.DTOs;

public record CreateNoteRequest(
    int? ChildId,
    string Title,
    string Content
);

public record UpdateNoteRequest(
    string? Title,
    string? Content
);

public record NoteResponse(
    int NoteId,
    int SpecialistId,
    int? ChildId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
