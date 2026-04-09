using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IEnumerable<Booking>> GetByParentIdAsync(int parentId);
    Task<IEnumerable<Booking>> GetBySpecialistIdAsync(int specialistId);
    Task<IEnumerable<Booking>> GetUpcomingBySpecialistIdAsync(int specialistId);
}
