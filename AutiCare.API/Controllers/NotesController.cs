using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/notes")]
[Authorize(Roles = "Doctor,Therapist")]
public class NotesController : BaseController
{
    private readonly INotesService _notesService;

    public NotesController(INotesService notesService)
    {
        _notesService = notesService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateNoteRequest request)
    {
        var note = await _notesService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = note.NoteId }, note);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var note = await _notesService.GetByIdAsync(id, GetUserId());
        return note == null ? NotFound() : Ok(note);
    }

    [HttpGet("my-notes")]
    public async Task<IActionResult> GetMyNotes()
    {
        var notes = await _notesService.GetMyNotesAsync(GetUserId());
        return Ok(notes);
    }

    [HttpGet("child/{childId}")]
    public async Task<IActionResult> GetChildNotes(int childId)
    {
        var notes = await _notesService.GetChildNotesAsync(childId, GetUserId());
        return Ok(notes);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateNoteRequest request)
    {
        await _notesService.UpdateAsync(id, request, GetUserId());
        return Ok(new { message = "Note updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _notesService.DeleteAsync(id, GetUserId());
        return NoContent();
    }
}
