using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class TreatmentPlanRepository : GenericRepository<TreatmentPlan>, ITreatmentPlanRepository
{
    public TreatmentPlanRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<TreatmentPlan>> GetByChildIdAsync(int childId) =>
        await _db.TreatmentPlans
            .Include(tp => tp.Specialist)
            .Include(tp => tp.Sessions.Where(s => !s.IsDeleted))
            .Where(tp => tp.ChildId == childId && !tp.IsDeleted)
            .OrderByDescending(tp => tp.StartDate)
            .ToListAsync();
    public async Task<TreatmentPlan?> GetWithDetailsAsync(int treatmentId) =>
        await _db.TreatmentPlans
            .Include(tp => tp.Child).ThenInclude(c => c.Parent)
            .Include(tp => tp.Specialist)
            .Include(tp => tp.Sessions)
            .FirstOrDefaultAsync(tp => tp.TreatmentId == treatmentId && !tp.IsDeleted);

    public async Task<IEnumerable<TreatmentPlan>> GetBySpecialistIdAsync(int specialistId) =>
    await _db.TreatmentPlans
        .Include(tp => tp.Child)
        .Include(tp => tp.Sessions)
        .Where(tp => tp.SpecialistId == specialistId && !tp.IsDeleted)
        .ToListAsync();

}
