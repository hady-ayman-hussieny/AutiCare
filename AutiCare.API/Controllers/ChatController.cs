using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using AutiCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AutiCare.API.Controllers;


// Chat endpoints for Parent ↔ Specialist messaging including Zoom-link session confirmation.

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public ChatController(ApplicationDbContext db) => _db = db;

    // ── Auth helpers ────────────────────────────────────────────────────────
    private Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(id))
            throw new UnauthorizedAccessException("Invalid or missing token.");
        return Guid.Parse(id);
    }

    private string GetRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? "";

    // ── Helpers to map Message entity → DTO ────────────────────────────────
    private static MessageResponse ToMessageResponse(Message m) => new(
        m.MessageId,
        m.ChatId,
        m.Content,
        m.SenderType,
        m.SenderUserId,
        m.MessageType,
        m.TimeStamp,
        m.IsRead
    );

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/chat/start
    // Parent starts (or retrieves) a chat with a Specialist.
    // Body: { "specialistId": 1 }
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("start")]
    [Authorize(Roles = "Parent")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> StartChat([FromBody] StartChatRequest request)
    {
        var userId = GetUserId();
        var parent = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
        if (parent == null) return Unauthorized(new { error = "Parent profile not found." });

        // Return existing active chat if one already exists
        var existing = await _db.Chats.FirstOrDefaultAsync(
            c => c.ParentId == parent.ParentId && c.SpecialistId == request.SpecialistId && c.IsActive);
        if (existing != null)
            return Ok(new { existing.ChatId, message = "Existing chat returned." });

        var chat = new Chat { ParentId = parent.ParentId, SpecialistId = request.SpecialistId };
        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();
        return Ok(new { chat.ChatId, message = "New chat created." });
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/chat/my-chats
    // Returns all chats for the authenticated user (Parent or Specialist).
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("my-chats")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = GetUserId();
        var role   = GetRole();

        if (role == "Parent")
        {
            var parent = await _db.Parents.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            if (parent == null) return Unauthorized(new { error = "Parent profile not found." });

            var chats = await _db.Chats
                .Include(c => c.Specialist)
                .Include(c => c.Messages.OrderByDescending(m => m.TimeStamp).Take(1))
                .Where(c => c.ParentId == parent.ParentId && c.IsActive)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return Ok(chats.Select(c => new
            {
                c.ChatId,
                SpecialistId   = c.SpecialistId,
                SpecialistName = c.Specialist?.Name ?? "",
                c.LastMessageAt,
                LastMessage     = c.Messages.FirstOrDefault()?.Content,
                LastMessageType = c.Messages.FirstOrDefault()?.MessageType
            }));
        }
        else // Doctor or Therapist
        {
            var specialist = await _db.Specialists.FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);
            if (specialist == null) return Unauthorized(new { error = "Specialist profile not found." });

            var chats = await _db.Chats
                .Include(c => c.Parent)
                .Include(c => c.Messages.OrderByDescending(m => m.TimeStamp).Take(1))
                .Where(c => c.SpecialistId == specialist.SpecialistId && c.IsActive)
                .OrderByDescending(c => c.LastMessageAt)
                .ToListAsync();

            return Ok(chats.Select(c => new
            {
                c.ChatId,
                ParentId        = c.ParentId,
                ParentName      = c.Parent?.Name ?? "",
                c.LastMessageAt,
                LastMessage     = c.Messages.FirstOrDefault()?.Content,
                LastMessageType = c.Messages.FirstOrDefault()?.MessageType
            }));
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // GET /api/chat/{chatId}/messages?page=1&size=50
    // Returns paginated messages for a chat (Parent or Specialist must be a participant).
    // ═══════════════════════════════════════════════════════════════════════
    [HttpGet("{chatId}/messages")]
    [ProducesResponseType(typeof(MessageResponse[]), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMessages(int chatId, int page = 1, int size = 50)
    {
        var userId = GetUserId();
        var role   = GetRole();

        var chat = await _db.Chats
            .Include(c => c.Parent)
            .Include(c => c.Specialist)
            .FirstOrDefaultAsync(c => c.ChatId == chatId);

        if (chat == null) return NotFound(new { error = "Chat not found." });

        // Authorization: caller must be a participant
        if (role == "Parent" && chat.Parent?.UserId != userId)
            return StatusCode(403, new { error = "You are not a participant in this chat." });

        if ((role == "Doctor" || role == "Therapist") && chat.Specialist?.UserId != userId)
            return StatusCode(403, new { error = "You are not a participant in this chat." });

        var messages = await _db.Messages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.TimeStamp)
            .Skip((page - 1) * size)
            .Take(size)
            .OrderBy(m => m.TimeStamp)
            .ToListAsync();

        return Ok(messages.Select(ToMessageResponse));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/chat/send
    // Sends a message in a chat. Both Parent and Specialist can send.
    // Body: { "chatId": 1, "content": "Hello", "messageType": "User" }
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("send")]
    [ProducesResponseType(typeof(MessageResponse), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        var role   = GetRole();

        var chat = await _db.Chats
            .Include(c => c.Parent)
            .Include(c => c.Specialist)
            .FirstOrDefaultAsync(c => c.ChatId == request.ChatId);

        if (chat == null) return NotFound(new { error = "Chat not found." });

        string senderType;
        if (role == "Parent")
        {
            if (chat.Parent?.UserId != userId)
                return StatusCode(403, new { error = "You are not authorized to send messages in this chat." });
            senderType = "Parent";
        }
        else if (role == "Doctor" || role == "Therapist")
        {
            if (chat.Specialist?.UserId != userId)
                return StatusCode(403, new { error = "You are not authorized to send messages in this chat." });
            senderType = "Specialist";
        }
        else
        {
            return StatusCode(403, new { error = "Invalid role." });
        }

        // Sanitise MessageType — only allow known values
        var allowedTypes = new[] { "User", "ZoomLink", "System" };
        var messageType  = allowedTypes.Contains(request.MessageType) ? request.MessageType : "User";

        var message = new Message
        {
            ChatId        = request.ChatId,
            Content       = request.Content,
            SenderType    = senderType,
            SenderUserId  = userId.ToString(),
            MessageType   = messageType,
            TimeStamp     = DateTime.UtcNow
        };

        chat.LastMessageAt = DateTime.UtcNow;
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return Ok(ToMessageResponse(message));
    }

    // ═══════════════════════════════════════════════════════════════════════
    // POST /api/chat/send-zoom-link
    // Specialist sends a formatted Zoom-link session confirmation message.
    // Body: { "chatId": 1, "zoomLink": "https://zoom.us/...", "confirmedDate": "2026-06-01", "confirmedTime": "14:00" }
    // ═══════════════════════════════════════════════════════════════════════
    [HttpPost("send-zoom-link")]
    [Authorize(Roles = "Doctor,Therapist")]
    [ProducesResponseType(typeof(MessageResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SendZoomLink([FromBody] SendZoomLinkRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ZoomLink))
            return BadRequest(new { error = "ZoomLink is required." });
        if (string.IsNullOrWhiteSpace(request.ConfirmedDate))
            return BadRequest(new { error = "ConfirmedDate is required." });
        if (string.IsNullOrWhiteSpace(request.ConfirmedTime))
            return BadRequest(new { error = "ConfirmedTime is required." });

        var userId = GetUserId();

        var chat = await _db.Chats
            .Include(c => c.Specialist)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.ChatId == request.ChatId);

        if (chat == null) return NotFound(new { error = "Chat not found." });

        if (chat.Specialist?.UserId != userId)
            return StatusCode(403, new { error = "You are not the specialist in this chat." });

        // Build a structured, readable message the parent will see
        var sb = new StringBuilder();
        sb.AppendLine("📅 Session Confirmed!");
        sb.AppendLine($"Date: {request.ConfirmedDate}");
        sb.AppendLine($"Time: {request.ConfirmedTime}");
        sb.AppendLine($"Zoom Link: {request.ZoomLink}");
        if (!string.IsNullOrWhiteSpace(request.Note))
            sb.AppendLine($"Note: {request.Note}");

        var message = new Message
        {
            ChatId       = request.ChatId,
            Content      = sb.ToString().Trim(),
            SenderType   = "Specialist",
            SenderUserId = userId.ToString(),
            MessageType  = "ZoomLink",
            TimeStamp    = DateTime.UtcNow
        };

        chat.LastMessageAt = DateTime.UtcNow;
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        return Ok(ToMessageResponse(message));
    }
}
