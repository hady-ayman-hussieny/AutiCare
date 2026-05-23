using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class DoctorRepository : GenericRepository<Specialist>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext db) : base(db) { }

    public async Task<Specialist?> GetByUserIdAsync(Guid userId) =>
        await _set.AsNoTracking().FirstOrDefaultAsync(s => s.UserId == userId && !s.IsDeleted);

    public async Task<(IEnumerable<Specialist> Specialists, int TotalRecords)> GetDoctorsAsync(PaginationFilter filter)
    {
        var query = _set.AsNoTracking().Where(s => !s.IsDeleted && s.Specialization != null);
        var total = await query.CountAsync();
        var data = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
        return (data, total);
    }
}
