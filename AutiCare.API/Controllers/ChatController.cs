using AutiCare.Domain.Entities;
using AutiCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutiCare.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public ChatController(ApplicationDbContext db) => _db = db;

    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("Invalid token");

        return Guid.Parse(id);
    }
    private string GetRole()   => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

    [HttpPost("start")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> StartChat([FromBody] int specialistId)
    {
        var userId = GetUserId();
        var parent = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (parent == null) return Unauthorized();

        var existing = await _db.Chats.FirstOrDefaultAsync(
            c => c.ParentId == parent.ParentId && c.SpecialistId == specialistId && c.IsActive);
        if (existing != null) return Ok(new { existing.ChatId });

        var chat = new Chat { ParentId = parent.ParentId, SpecialistId = specialistId };
        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();
        return Ok(new { chat.ChatId });
    }

    [HttpGet("my-chats")]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = GetUserId();
        var role   = GetRole();

        if (role == "Parent")
        {
            var parent = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == userId);
            if (parent == null) return Unauthorized();

            var chats = await _db.Chats
                .Include(c => c.Specialist)
                .Include(c => c.Messages.OrderByDescending(m => m.TimeStamp).Take(1))
                .Where(c => c.ParentId == parent.ParentId && c.IsActive)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return Ok(chats.Select(c => new
            {
                c.ChatId,
                SpecialistName = c.Specialist.Name,
                c.LastMessageAt,
                LastMessage = c.Messages.FirstOrDefault()?.Content
            }));
        }
        else
        {
            var specialist = await _db.Specialists.FirstOrDefaultAsync(s => s.UserId == userId);
            if (specialist == null) return Unauthorized();

            var chats = await _db.Chats
                .Include(c => c.Parent)
                .Include(c => c.Messages.OrderByDescending(m => m.TimeStamp).Take(1))
                .Where(c => c.SpecialistId == specialist.SpecialistId && c.IsActive)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return Ok(chats.Select(c => new
            {
                c.ChatId,
                ParentName = c.Parent.Name,
                c.LastMessageAt,
                LastMessage = c.Messages.FirstOrDefault()?.Content
            }));
        }
    }

    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetMessages(int chatId, int page = 1, int size = 50)
    {
        var userId = GetUserId();
        var role = GetRole();

        var chat = await _db.Chats
            .Include(c => c.Parent)
            .Include(c => c.Specialist)
            .FirstOrDefaultAsync(c => c.ChatId == chatId);

        if (chat == null) return NotFound();

        if (role == "Parent" && chat.Parent?.UserId != userId)
            return Unauthorized();
            
        if ((role == "Doctor" || role == "Therapist") && chat.Specialist?.UserId != userId)
            return Unauthorized();

        var messages = await _db.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.TimeStamp)
            .Skip((page - 1) * size).Take(size)
            .OrderBy(m => m.TimeStamp)
            .ToListAsync();

        return Ok(messages);
    }
}
