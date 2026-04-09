using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Booking>> GetByParentIdAsync(int parentId)
    {
        return await _set
            .Include(b => b.Parent)
            .Include(b => b.Specialist)
            .Include(b => b.Child)
            .Where(b => b.ParentId == parentId && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBySpecialistIdAsync(int specialistId)
    {
        return await _set
            .Include(b => b.Parent)
            .Include(b => b.Specialist)
            .Include(b => b.Child)
            .Where(b => b.SpecialistId == specialistId && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetUpcomingBySpecialistIdAsync(int specialistId)
    {
        return await _set
            .Include(b => b.Parent)
            .Include(b => b.Specialist)
            .Include(b => b.Child)
            .Where(b => b.SpecialistId == specialistId && !b.IsDeleted && b.BookingDate >= System.DateTime.UtcNow.Date)
            .OrderBy(b => b.BookingDate)
            .ToListAsync();
    }
}
