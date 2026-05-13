using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(ApplicationDbContext db) : base(db) { }
}
