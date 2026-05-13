using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IDashboardRepository
{
    Task<SpecialistDashboardResponse> GetSpecialistDashboardDataAsync(int specialistId);
}
