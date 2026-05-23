using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class SessionRepository : GenericRepository<Session>, ISessionRepository
{
    public SessionRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Session>> GetByTreatmentIdAsync(int treatmentId) =>
        await _db.Sessions
            .Where(s => s.TreatmentId == treatmentId && !s.IsDeleted)
            .OrderByDescending(s => s.SessionDate)
            .ToListAsync();
}
