using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class TreatmentService : ITreatmentService
{
    private readonly ITreatmentPlanRepository _treatmentRepo;
    private readonly ISessionRepository _sessionRepo;
    private readonly IDoctorRepository _doctorRepo;
    private readonly IParentRepository _parentRepo;
    private readonly IChildRepository _childRepo;

    public TreatmentService(
        ITreatmentPlanRepository treatmentRepo,
        ISessionRepository sessionRepo,
        IDoctorRepository doctorRepo,
        IParentRepository parentRepo,
        IChildRepository childRepo)
    {
        _treatmentRepo = treatmentRepo;
        _sessionRepo = sessionRepo;
        _doctorRepo = doctorRepo;
        _parentRepo = parentRepo;
        _childRepo = childRepo;
    }

    public async Task<IEnumerable<TreatmentPlanResponse>> GetMyPlansAsync(Guid specialistUserId)
    {
        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId )
            ?? throw new KeyNotFoundException("Specialist not found");

        var plans = await _treatmentRepo.GetBySpecialistIdAsync(specialist.SpecialistId);

        return plans.Select(p => new TreatmentPlanResponse(
            p.TreatmentId, p.ChildId, p.Child.FirstName + " " + p.Child.LastName,
            p.SpecialistId, p.Specialist.Name, p.Goal, p.Notes, p.Progress,
            p.StartDate, p.EndDate, p.Sessions?.Count ?? 0, p.CreatedAt
        ));
    }

    public async Task<TreatmentPlanResponse> CreatePlanAsync(Guid specialistUserId, CreateTreatmentPlanRequest request)
    {
        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId)
            ?? throw new KeyNotFoundException("Specialist not found");

        var child = await _childRepo.GetByIdAsync(request.ChildId)
            ?? throw new KeyNotFoundException("Child not found");

        var plan = new TreatmentPlan
        {
            ChildId = request.ChildId,
            SpecialistId = specialist.SpecialistId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Goal = request.Goal,
            Notes = request.Notes
        };

        await _treatmentRepo.AddAsync(plan);
        await _treatmentRepo.SaveChangesAsync();

        var full = await _treatmentRepo.GetWithDetailsAsync(plan.TreatmentId);
        return ToPlanResponse(full!);
    }

    public async Task<TreatmentPlanResponse?> GetPlanAsync(int treatmentId, Guid userId)
    {
        var plan = await _treatmentRepo.GetByIdAsync(treatmentId);
        var specialist = await _doctorRepo.GetByUserIdAsync(userId);

        if (plan == null || specialist == null || plan.SpecialistId != specialist.SpecialistId)
            return null;

        return ToPlanResponse(plan);
    }

    public async Task<IEnumerable<TreatmentPlanResponse>> GetByChildAsync(int childId, Guid userId, string role)
    {
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            var child = await _childRepo.GetByIdAsync(childId);
            if (parent == null || child == null || child.ParentId != parent.ParentId)
                throw new UnauthorizedAccessException("Unauthorized approach to child's treatment plans.");
        }

        var plans = await _treatmentRepo.GetByChildIdAsync(childId);

        if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _doctorRepo.GetByUserIdAsync(userId);
            if (specialist == null) throw new UnauthorizedAccessException();
            plans = plans.Where(p => p.SpecialistId == specialist.SpecialistId).ToList();
        }

        return plans.Select(ToPlanResponse);
    }

    public async Task UpdateAsync(int treatmentId, UpdateTreatmentPlanRequest request, Guid specialistUserId)
    {
        var plan = await _treatmentRepo.GetByIdAsync(treatmentId)
            ?? throw new KeyNotFoundException("Plan not found");

        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null || plan.SpecialistId != specialist.SpecialistId)
            throw new UnauthorizedAccessException("You can only modify your own treatment plans.");

        if (request.Goal != null) plan.Goal = request.Goal;
        if (request.Notes != null) plan.Notes = request.Notes;
        if (request.Progress != null) plan.Progress = request.Progress;
        if (request.EndDate.HasValue) plan.EndDate = request.EndDate;

        _treatmentRepo.Update(plan);
        await _treatmentRepo.SaveChangesAsync();
    }

    public async Task<SessionResponse> AddSessionAsync(Guid specialistUserId, CreateSessionRequest request)
    {
        var plan = await _treatmentRepo.GetByIdAsync(request.TreatmentId)
            ?? throw new KeyNotFoundException("Treatment plan not found");

        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null || plan.SpecialistId != specialist.SpecialistId)
            throw new UnauthorizedAccessException("Cannot add sessions to another specialist's plan.");

        var session = new Session
        {
            TreatmentId = request.TreatmentId,
            SessionDate = request.SessionDate,
            SessionTime = request.SessionTime,
            SessionNotes = request.SessionNotes,
            ActivityNotes = request.ActivityNotes,
            Report = request.Report
        };

        await _sessionRepo.AddAsync(session);
        await _sessionRepo.SaveChangesAsync();
        return ToSessionResponse(session);
    }

    public async Task<IEnumerable<SessionResponse>> GetSessionsAsync(Guid userId, string role, int treatmentId)
    {
        var plan = await _treatmentRepo.GetByIdAsync(treatmentId)
            ?? throw new KeyNotFoundException("Treatment plan not found");
            
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            var child = await _childRepo.GetByIdAsync(plan.ChildId);
            if (parent == null || child == null || child.ParentId != parent.ParentId)
                 throw new UnauthorizedAccessException("Unauthorized.");
        }
        else if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _doctorRepo.GetByUserIdAsync(userId);
            if (specialist == null || plan.SpecialistId != specialist.SpecialistId)
                 throw new UnauthorizedAccessException("Unauthorized.");
        }

        var sessions = await _sessionRepo.GetByTreatmentIdAsync(treatmentId);
        return sessions.Select(ToSessionResponse);
    }

    public async Task UpdateSessionAsync(int sessionId, UpdateSessionRequest request, Guid specialistUserId)
    {
        var session = await _sessionRepo.GetByIdAsync(sessionId)
            ?? throw new KeyNotFoundException("Session not found");

        var plan = await _treatmentRepo.GetByIdAsync(session.TreatmentId);
        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId);

        if (plan == null || specialist == null || plan.SpecialistId != specialist.SpecialistId)
            throw new UnauthorizedAccessException("Cannot update a session for an unassigned plan.");

        if (request.SessionNotes != null) session.SessionNotes = request.SessionNotes;
        if (request.ActivityNotes != null) session.ActivityNotes = request.ActivityNotes;
        if (request.Report != null) session.Report = request.Report;

        _sessionRepo.Update(session);
        await _sessionRepo.SaveChangesAsync();
    }

    private static TreatmentPlanResponse ToPlanResponse(TreatmentPlan p) => new(
        p.TreatmentId,
        p.ChildId,
        p.Child != null ? $"{p.Child.FirstName} {p.Child.LastName}" : "",
        p.SpecialistId,
        p.Specialist?.Name ?? "",
        p.Goal, p.Notes, p.Progress,
        p.StartDate, p.EndDate,
        p.Sessions?.Count(s => !s.IsDeleted) ?? 0,
        p.CreatedAt);

    private static SessionResponse ToSessionResponse(Session s) => new(
        s.SessionId, s.TreatmentId, s.SessionDate, s.SessionTime,
        s.SessionNotes, s.ActivityNotes, s.Report, s.CreatedAt);
}
