using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Interfaces;

public interface INoteRepository : IGenericRepository<SystemNote>
{
    Task<IEnumerable<SystemNote>> GetBySpecialistIdAsync(int specialistId);
    Task<IEnumerable<SystemNote>> GetByChildIdAsync(int childId);
}
