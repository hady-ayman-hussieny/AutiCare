using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class ChatRepository : GenericRepository<Chat>, IChatRepository
{
    public ChatRepository(ApplicationDbContext db) : base(db) { }

    public async Task<Chat?> GetChatByParticipantsAsync(int parentId, int specialistId)
    {
        return await _set.FirstOrDefaultAsync(c => c.ParentId == parentId && c.SpecialistId == specialistId);
    }

    public async Task<int> GetPendingMessagesCountAsync(int specialistId)
    {
        return await _db.Messages
            .Where(m => m.Chat.SpecialistId == specialistId && !m.IsRead && m.SenderType == "Parent")
            .CountAsync();
    }
}
