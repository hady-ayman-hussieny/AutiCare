using AutiCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutiCare.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public NotificationsController(ApplicationDbContext db) => _db = db;

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userId = GetUserId();
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new
            {
                n.NotificationId,
                n.Title,
                Message = n.MessageText,
                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var n = await _db.Notifications.FindAsync(id);
        if (n == null) return NotFound();
        
        if (n.UserId != GetUserId())
            return Unauthorized();
            
        n.IsRead = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetUserId();
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        unread.ForEach(n => n.IsRead = true);
        await _db.SaveChangesAsync();
        return Ok(new { count = unread.Count });
    }
}
