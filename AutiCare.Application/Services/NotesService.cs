using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class NotesService : INotesService
{
    private readonly INoteRepository _noteRepo;
    private readonly IDoctorRepository _specialistRepo;
    private readonly IChildRepository _childRepo;

    public NotesService(INoteRepository noteRepo, IDoctorRepository specialistRepo, IChildRepository childRepo)
    {
        _noteRepo = noteRepo;
        _specialistRepo = specialistRepo;
        _childRepo = childRepo;
    }

    public async Task<NoteResponse> CreateAsync(Guid specialistUserId, CreateNoteRequest request)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId)
            ?? throw new KeyNotFoundException("Specialist not found");

        if (request.ChildId.HasValue)
        {
            var child = await _childRepo.GetByIdAsync(request.ChildId.Value)
                ?? throw new KeyNotFoundException("Child not found");
        }

        var note = new SystemNote
        {
            SpecialistId = specialist.SpecialistId,
            ChildId = request.ChildId,
            Title = request.Title,
            Content = request.Content
        };

        await _noteRepo.AddAsync(note);
        await _noteRepo.SaveChangesAsync();
        return ToResponse(note);
    }

    public async Task<NoteResponse?> GetByIdAsync(int noteId, Guid specialistUserId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null) return null;

        var note = await _noteRepo.GetByIdAsync(noteId);
        if (note == null || note.SpecialistId != specialist.SpecialistId) return null;

        return ToResponse(note);
    }

    public async Task<IEnumerable<NoteResponse>> GetMyNotesAsync(Guid specialistUserId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null) return Enumerable.Empty<NoteResponse>();

        var notes = await _noteRepo.GetBySpecialistIdAsync(specialist.SpecialistId);
        return notes.Select(ToResponse);
    }

    public async Task<IEnumerable<NoteResponse>> GetChildNotesAsync(int childId, Guid specialistUserId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null) return Enumerable.Empty<NoteResponse>();

        var notes = await _noteRepo.GetByChildIdAsync(childId);
        // Ensure they only see their own notes
        return notes.Where(n => n.SpecialistId == specialist.SpecialistId).Select(ToResponse);
    }

    public async Task UpdateAsync(int noteId, UpdateNoteRequest request, Guid specialistUserId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId)
            ?? throw new KeyNotFoundException("Specialist not found");

        var note = await _noteRepo.GetByIdAsync(noteId)
            ?? throw new KeyNotFoundException("Note not found");

        if (note.SpecialistId != specialist.SpecialistId) throw new UnauthorizedAccessException();

        if (request.Title != null) note.Title = request.Title;
        if (request.Content != null) note.Content = request.Content;
        note.UpdatedAt = DateTime.UtcNow;

        _noteRepo.Update(note);
        await _noteRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int noteId, Guid specialistUserId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId)
            ?? throw new KeyNotFoundException("Specialist not found");

        var note = await _noteRepo.GetByIdAsync(noteId)
            ?? throw new KeyNotFoundException("Note not found");

        if (note.SpecialistId != specialist.SpecialistId) throw new UnauthorizedAccessException();

        note.IsDeleted = true;
        note.DeletedAt = DateTime.UtcNow;
        note.DeletedBy = specialistUserId.ToString();

        _noteRepo.Update(note);
        await _noteRepo.SaveChangesAsync();
    }

    private static NoteResponse ToResponse(SystemNote n) => new(
        n.NoteId, n.SpecialistId, n.ChildId, n.Title, n.Content, n.CreatedAt, n.UpdatedAt
    );
}
