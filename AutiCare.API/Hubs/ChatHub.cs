using AutiCare.Domain.Entities;
using AutiCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutiCare.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _db;

    public ChatHub(ApplicationDbContext db) => _db = db;

    public async Task JoinChat(int chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
    }

    public async Task SendMessage(int chatId, string content)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var role   = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";

        var message = new Message
        {
            ChatId       = chatId,
            Content      = content,
            SenderUserId = userId,
            SenderType   = role == "Parent" ? "Parent" : "Specialist"
        };

        _db.Messages.Add(message);

        var chat = await _db.Chats.FindAsync(chatId);
        if (chat != null) chat.LastMessageAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", new
        {
            message.MessageId,
            message.ChatId,
            message.Content,
            message.SenderType,
            message.SenderUserId,
            message.TimeStamp
        });
    }

    public async Task MarkAsRead(int chatId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ;
        var role   = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var oppositeType = role == "Parent" ? "Specialist" : "Parent";

        var unread = await _db.Messages
            .Where(m => m.ChatId == chatId && !m.IsRead && m.SenderType == oppositeType)
            .ToListAsync();

        unread.ForEach(m => m.IsRead = true);
        await _db.SaveChangesAsync();
    }
}
