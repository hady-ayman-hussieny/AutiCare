using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface INotesService
{
    Task<NoteResponse> CreateAsync(Guid specialistUserId, CreateNoteRequest request);
    Task<NoteResponse?> GetByIdAsync(int noteId, Guid specialistUserId);
    Task<IEnumerable<NoteResponse>> GetMyNotesAsync(Guid specialistUserId);
    Task<IEnumerable<NoteResponse>> GetChildNotesAsync(int childId, Guid specialistUserId);
    Task UpdateAsync(int noteId, UpdateNoteRequest request, Guid specialistUserId);
    Task DeleteAsync(int noteId, Guid specialistUserId);
}
