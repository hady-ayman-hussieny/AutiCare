using System;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _db;

    public DashboardRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SpecialistDashboardResponse> GetSpecialistDashboardDataAsync(int specialistId)
    {
        var totalPatients = await _db.TreatmentPlans
            .Where(p => p.SpecialistId == specialistId)
            .Select(p => p.ChildId)
            .Distinct()
            .CountAsync();

        var upcomingSessions = await _db.Sessions
            .Where(s => s.SpecialistId == specialistId && s.SessionDate >= DateTime.UtcNow.Date)
            .OrderBy(s => s.SessionDate)
            .Take(5)
            .Select(s => new UpcomingSessionDto(
                s.TreatmentPlan != null ? (s.TreatmentPlan.Child.FirstName + " " + s.TreatmentPlan.Child.LastName) : (s.Parent.Children.FirstOrDefault() != null ? s.Parent.Children.First().FirstName : "Unknown"),
                s.SessionDate,
                s.Duration,
                s.MeetingLink
            )).ToListAsync();

        var pendingMessages = await _db.Messages
            .Where(m => m.Chat.SpecialistId == specialistId && !m.IsRead && m.SenderType == "Parent")
            .CountAsync();

        var latestNotes = await _db.Sessions
            .Where(s => s.SpecialistId == specialistId && s.SessionNotes != null)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new LatestNoteDto(s.SessionNotes, s.ActivityNotes))
            .ToListAsync();

        var plans = await _db.TreatmentPlans
            .Where(p => p.SpecialistId == specialistId)
            .Include(p => p.Child)
            .Include(p => p.Specialist)
            .ToListAsync();

        var patientCards = plans
            .Select(p => new PatientCardDto(
                p.Child.FirstName + " " + p.Child.LastName,
                (int)((DateTime.UtcNow - p.Child.DateOfBirth).TotalDays / 365.25),
                "Level 1",
                p.Specialist.Name,
                int.TryParse(p.Progress, out var prog) ? prog : 0
            )).ToList();

        return new SpecialistDashboardResponse(
            totalPatients,
            upcomingSessions.Count,
            pendingMessages,
            0, // ReportsToReviewCount
            upcomingSessions,
            latestNotes,
            patientCards
        );
    }
}
