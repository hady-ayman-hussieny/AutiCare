using System;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;

namespace AutiCare.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IParentRepository _parentRepo;
    private readonly IDoctorRepository _specialistRepo;
    private readonly IChildRepository _childRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ITreatmentPlanRepository _treatmentPlanRepo;
    private readonly IAssessmentRepository _assessmentRepo;

    public DashboardService(
        IParentRepository parentRepo,
        IDoctorRepository specialistRepo,
        IChildRepository childRepo,
        IBookingRepository bookingRepo,
        ITreatmentPlanRepository treatmentPlanRepo,
        IAssessmentRepository assessmentRepo)
    {
        _parentRepo = parentRepo;
        _specialistRepo = specialistRepo;
        _childRepo = childRepo;
        _bookingRepo = bookingRepo;
        _treatmentPlanRepo = treatmentPlanRepo;
        _assessmentRepo = assessmentRepo;
    }

    public async Task<ParentDashboardResponse> GetParentDashboardAsync(Guid userId)
    {
        var parent = await _parentRepo.GetByUserIdAsync(userId);
        if (parent == null) return new ParentDashboardResponse(0, 0, 0, 0);

        var children = await _childRepo.GetByParentIdAsync(parent.ParentId);
        var bookings = await _bookingRepo.GetByParentIdAsync(parent.ParentId);
        
        int upcomingBookings = bookings.Count(b => b.BookingDate >= DateTime.UtcNow.Date);
        
        int completedTests = 0;
        foreach(var child in children)
        {
            var results = await _assessmentRepo.GetAllResultsByChildIdAsync(child.ChildId);
            completedTests += results.Count();
        }

        return new ParentDashboardResponse(children.Count(), upcomingBookings, completedTests, 0); // Unread notifications can be added later
    }

    public async Task<SpecialistDashboardResponse> GetSpecialistDashboardAsync(Guid userId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(userId);
        if (specialist == null) return new SpecialistDashboardResponse(0, 0, 0, 0);

        var bookings = await _bookingRepo.GetUpcomingBySpecialistIdAsync(specialist.SpecialistId);
        var plans = await _treatmentPlanRepo.GetBySpecialistIdAsync(specialist.SpecialistId);

        int totalPatients = plans.Select(p => p.ChildId).Distinct().Count();

        return new SpecialistDashboardResponse(totalPatients, bookings.Count(), plans.Count(), 0);
    }
}
