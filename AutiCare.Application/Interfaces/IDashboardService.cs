using System;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IDashboardService
{
    Task<ParentDashboardResponse> GetParentDashboardAsync(Guid userId);
    Task<SpecialistDashboardResponse> GetSpecialistDashboardAsync(Guid userId);
}
