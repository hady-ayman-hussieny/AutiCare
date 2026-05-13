using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    new Task<Booking?> GetByIdAsync(int id);
    Task<IEnumerable<Booking>> GetByParentIdAsync(int parentId);
    Task<IEnumerable<Booking>> GetBySpecialistIdAsync(int specialistId);
    Task<IEnumerable<Booking>> GetUpcomingBySpecialistIdAsync(int specialistId);
}
