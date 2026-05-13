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
    private readonly IChatRepository _chatRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly ISignalRService _signalRService;

    public TreatmentService(
        ITreatmentPlanRepository treatmentRepo,
        ISessionRepository sessionRepo,
        IDoctorRepository doctorRepo,
        IParentRepository parentRepo,
        IChildRepository childRepo,
        IChatRepository chatRepo,
        IMessageRepository messageRepo,
        ISignalRService signalRService)
    {
        _treatmentRepo = treatmentRepo;
        _sessionRepo = sessionRepo;
        _doctorRepo = doctorRepo;
        _parentRepo = parentRepo;
        _childRepo = childRepo;
        _chatRepo = chatRepo;
        _messageRepo = messageRepo;
        _signalRService = signalRService;
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
        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null) throw new UnauthorizedAccessException("Specialist not found.");

        if (request.SpecialistId != specialist.SpecialistId)
            throw new UnauthorizedAccessException("Cannot book session for another specialist.");

        if (request.TreatmentId.HasValue)
        {
            var plan = await _treatmentRepo.GetByIdAsync(request.TreatmentId.Value);
            if (plan == null) throw new KeyNotFoundException("Treatment plan not found.");
            if (plan.SpecialistId != specialist.SpecialistId)
                throw new UnauthorizedAccessException("Cannot add sessions to another specialist's plan.");
        }

        var meetingLink = !string.IsNullOrWhiteSpace(request.MeetingLink) 
            ? request.MeetingLink 
            : $"https://zoom.us/j/{new Random().Next(100000000, 999999999)}";

        var session = new Session
        {
            TreatmentId = request.TreatmentId,
            ParentId = request.ParentId,
            SpecialistId = request.SpecialistId,
            SessionDate = request.SessionDate,
            SessionTime = request.SessionTime,
            Duration = request.Duration,
            MeetingLink = meetingLink,
            SessionNotes = request.SessionNotes,
            ActivityNotes = request.ActivityNotes,
            Report = request.Report
        };

        await _sessionRepo.AddAsync(session);
        await _sessionRepo.SaveChangesAsync();

        // ── Auto Session Chat Message + SignalR ──
        var chat = await _chatRepo.GetChatByParticipantsAsync(request.ParentId, request.SpecialistId);
        if (chat == null)
        {
            chat = new Chat { ParentId = request.ParentId, SpecialistId = request.SpecialistId };
            await _chatRepo.AddAsync(chat);
            await _chatRepo.SaveChangesAsync();
        }

        var formattedDate = request.SessionDate.ToString("dd MMM yyyy");
        var formattedTime = request.SessionTime.HasValue ? DateTime.Today.Add(request.SessionTime.Value).ToString("h:mm tt") : "TBD";
        var content = $"Session confirmed with Dr. {specialist.Name}\n\nDate: {formattedDate}\nTime: {formattedTime}\n\nMeeting Link:\n{meetingLink}";

        var message = new Message
        {
            ChatId = chat.ChatId,
            Content = content,
            SenderType = "System",
            SenderUserId = "System",
            MessageType = "System",
            TimeStamp = DateTime.UtcNow
        };
        await _messageRepo.AddAsync(message);
        chat.LastMessageAt = DateTime.UtcNow;
        _chatRepo.Update(chat);
        await _messageRepo.SaveChangesAsync();

        await _signalRService.SendSystemMessageAsync(chat.ChatId, message.MessageId, message.Content, message.TimeStamp);

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

        var specialist = await _doctorRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null || session.SpecialistId != specialist.SpecialistId)
            throw new UnauthorizedAccessException("Cannot update a session not assigned to you.");

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
        s.SessionId, s.TreatmentId, s.ParentId, s.SpecialistId, s.SessionDate, s.SessionTime, s.Duration,
        s.MeetingLink, s.SessionNotes, s.ActivityNotes, s.Report, s.CreatedAt);
}
