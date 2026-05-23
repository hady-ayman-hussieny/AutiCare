using System;
using System.Threading.Tasks;

namespace AutiCare.Application.Interfaces;

public interface ISignalRService
{
    Task SendSystemMessageAsync(int chatId, int messageId, string content, DateTime timeStamp);
}
