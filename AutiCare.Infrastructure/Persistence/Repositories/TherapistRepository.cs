using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class TherapistRepository : GenericRepository<Specialist>, ITherapistRepository
{
    public TherapistRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Specialist>> GetTherapistsAsync() =>
        await _db.Specialists.Where(s => !s.IsDeleted).ToListAsync();
}
