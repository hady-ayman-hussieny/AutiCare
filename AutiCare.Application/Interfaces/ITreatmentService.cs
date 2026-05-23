using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface ITreatmentService
{
    Task<TreatmentPlanResponse> CreatePlanAsync(Guid specialistUserId, CreateTreatmentPlanRequest request);
    Task<TreatmentPlanResponse?> GetPlanAsync(int treatmentId,Guid userId);
    Task<IEnumerable<TreatmentPlanResponse>> GetByChildAsync(int childId, Guid userId, string role);
    Task<IEnumerable<TreatmentPlanResponse>> GetMyPlansAsync(Guid specialistUserId);
    Task UpdateAsync(int treatmentId, UpdateTreatmentPlanRequest request, Guid specialistUserId);
    Task<SessionResponse> AddSessionAsync(Guid specialistUserId, CreateSessionRequest request);
    Task<IEnumerable<SessionResponse>> GetSessionsAsync(Guid userId, string role, int treatmentId);
    Task UpdateSessionAsync(int sessionId, UpdateSessionRequest request, Guid specialistUserId);
}
