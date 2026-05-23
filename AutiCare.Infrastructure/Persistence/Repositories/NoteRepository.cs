using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class NoteRepository : GenericRepository<SystemNote>, INoteRepository
{
    public NoteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<SystemNote>> GetBySpecialistIdAsync(int specialistId)
    {
        return await _set
            .Where(n => n.SpecialistId == specialistId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<SystemNote>> GetByChildIdAsync(int childId)
    {
        return await _set
            .Where(n => n.ChildId == childId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }
}
