using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class ParentRepository : GenericRepository<Parent>, IParentRepository
{
    public ParentRepository(ApplicationDbContext db) : base(db) { }

    public async Task<Parent?> GetByUserIdAsync(Guid userId) =>
        await _db.Parents.FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);

    public async Task<Parent?> GetByEmailAsync(string email) =>
        await _db.Parents.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted);
}
