using System.Threading.Tasks;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Interfaces;

public interface IChatRepository : IGenericRepository<Chat>
{
    Task<Chat?> GetChatByParticipantsAsync(int parentId, int specialistId);
    Task<int> GetPendingMessagesCountAsync(int specialistId);
}
