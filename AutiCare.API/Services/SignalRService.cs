using System;
using System.Threading.Tasks;
using AutiCare.API.Hubs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AutiCare.API.Services;

public class SignalRService : ISignalRService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRService(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendSystemMessageAsync(int chatId, int messageId, string content, DateTime timeStamp)
    {
        await _hubContext.Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", new
        {
            MessageId = messageId,
            ChatId = chatId,
            Content = content,
            SenderType = "System",
            SenderUserId = "System",
            TimeStamp = timeStamp
        });
    }
}
