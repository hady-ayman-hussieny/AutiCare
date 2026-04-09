using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class ChildRepository : GenericRepository<Child>, IChildRepository
{
    public ChildRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Child>> GetByParentIdAsync(int parentId) =>
        await _db.Children.Where(c => c.ParentId == parentId && !c.IsDeleted).ToListAsync();

    public async Task<Child?> GetWithDetailsAsync(int childId) =>
        await _db.Children
            .Include(c => c.TreatmentPlans.Where(tp => !tp.IsDeleted))
                .ThenInclude(tp => tp.Specialist)
            .Include(c => c.ParentTests)
                .ThenInclude(pt => pt.AIResult)
            .FirstOrDefaultAsync(c => c.ChildId == childId && !c.IsDeleted);
}
