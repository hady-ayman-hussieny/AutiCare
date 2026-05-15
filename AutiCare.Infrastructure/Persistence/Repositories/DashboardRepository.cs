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
        // ── Total unique patients ──────────────────────────────────────────
        // Union children from TreatmentPlans AND from confirmed Bookings so the
        // count is accurate even in the booking-first workflow (no TreatmentPlan required).
        var childrenFromPlans = await _db.TreatmentPlans
            .Where(p => p.SpecialistId == specialistId)
            .Select(p => p.ChildId)
            .Distinct()
            .ToListAsync();

        var childrenFromBookings = await _db.Bookings
            .Where(b => b.SpecialistId == specialistId
                     && !b.IsDeleted
                     && b.ChildId != null
                     && (b.Status == "Confirmed" || b.Status == "Completed"))
            .Select(b => b.ChildId!.Value)
            .Distinct()
            .ToListAsync();

        var totalPatients = childrenFromPlans.Union(childrenFromBookings).Distinct().Count();

        // ── Upcoming sessions from Sessions table (treatment-plan based) ───
        var upcomingFromSessions = await _db.Sessions
            .Where(s => s.SpecialistId == specialistId && s.SessionDate >= DateTime.UtcNow.Date)
            .OrderBy(s => s.SessionDate)
            .Take(5)
            .Select(s => new UpcomingSessionDto(
                s.TreatmentPlan != null
                    ? (s.TreatmentPlan.Child.FirstName + " " + s.TreatmentPlan.Child.LastName)
                    : (s.Parent.Children.FirstOrDefault() != null
                        ? s.Parent.Children.First().FirstName
                        : "Unknown"),
                s.SessionDate,
                s.Duration,
                s.MeetingLink
            ))
            .ToListAsync();

        // ── Upcoming confirmed bookings (booking-first workflow) ───────────
        var upcomingFromBookings = await _db.Bookings
            .Include(b => b.Child)
            .Include(b => b.Parent)
            .Where(b => b.SpecialistId == specialistId
                     && !b.IsDeleted
                     && b.Status == "Confirmed"
                     && b.PreferredDate >= DateTime.UtcNow.Date)
            .OrderBy(b => b.PreferredDate)
            .Take(5)
            .ToListAsync();

        var upcomingFromBookingsDtos = upcomingFromBookings.Select(b => new UpcomingSessionDto(
            b.Child != null
                ? $"{b.Child.FirstName} {b.Child.LastName}"
                : b.Parent?.Name ?? "Unknown",
            b.PreferredDate,
            null,           // Duration not stored on Booking
            null            // MeetingLink sent via Chat, not stored on Booking
        )).ToList();

        // Merge and take top 5 by date
        var upcomingSessions = upcomingFromSessions
            .Concat(upcomingFromBookingsDtos)
            .OrderBy(s => s.SessionDate)
            .Take(5)
            .ToList();

        // ── Pending messages from parents ──────────────────────────────────
        var pendingMessages = await _db.Messages
            .Where(m => m.Chat.SpecialistId == specialistId
                     && !m.IsRead
                     && m.SenderType == "Parent")
            .CountAsync();

        // ── Latest session notes ───────────────────────────────────────────
        var latestNotes = await _db.Sessions
            .Where(s => s.SpecialistId == specialistId && s.SessionNotes != null)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new LatestNoteDto(s.SessionNotes, s.ActivityNotes))
            .ToListAsync();

        // ── Patient cards (from TreatmentPlans) ───────────────────────────
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
            0,                // ReportsToReviewCount
            upcomingSessions,
            latestNotes,
            patientCards
        );
    }
}
