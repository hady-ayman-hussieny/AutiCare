using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface ITreatmentPlanRepository : IGenericRepository<TreatmentPlan>
{
    Task<IEnumerable<TreatmentPlan>> GetByChildIdAsync(int childId);
    Task<IEnumerable<TreatmentPlan>> GetBySpecialistIdAsync(int specialistId);
    Task<TreatmentPlan?> GetWithDetailsAsync(int treatmentId);
}
