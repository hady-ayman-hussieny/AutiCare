using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IDoctorRepository : IGenericRepository<Specialist>
{
    Task<Specialist?> GetByUserIdAsync(Guid userId);
    Task<(IEnumerable<Specialist> Specialists, int TotalRecords)> GetDoctorsAsync(PaginationFilter filter);
}
