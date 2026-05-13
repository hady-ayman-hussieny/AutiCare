using System;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IParentRepository _parentRepo;
    private readonly IDoctorRepository _specialistRepo;
    private readonly IChildRepository _childRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly ITreatmentPlanRepository _treatmentPlanRepo;
    private readonly IGenericRepository<PredictionResult> _predictionRepo;
    private readonly IDashboardRepository _dashboardRepo;

    public DashboardService(
        IParentRepository parentRepo,
        IDoctorRepository specialistRepo,
        IChildRepository childRepo,
        IBookingRepository bookingRepo,
        ITreatmentPlanRepository treatmentPlanRepo,
        IGenericRepository<PredictionResult> predictionRepo,
        IDashboardRepository dashboardRepo)
    {
        _parentRepo = parentRepo;
        _specialistRepo = specialistRepo;
        _childRepo = childRepo;
        _bookingRepo = bookingRepo;
        _treatmentPlanRepo = treatmentPlanRepo;
        _predictionRepo = predictionRepo;
        _dashboardRepo = dashboardRepo;
    }

    public async Task<ParentDashboardResponse> GetParentDashboardAsync(Guid userId)
    {
        var parent = await _parentRepo.GetByUserIdAsync(userId);
        if (parent == null) return new ParentDashboardResponse(0, 0, 0, 0);

        var children = await _childRepo.GetByParentIdAsync(parent.ParentId);
        var bookings = await _bookingRepo.GetByParentIdAsync(parent.ParentId);
        
        int upcomingBookings = bookings.Count(b => b.PreferredDate >= DateTime.UtcNow.Date);
        
        // Count completed screenings from PredictionResults table
        var allPredictions = await _predictionRepo.GetAllAsync();
        var childIds = children.Select(c => c.ChildId).ToHashSet();
        int completedTests = allPredictions.Count(p => childIds.Contains(p.ChildId));

        return new ParentDashboardResponse(children.Count(), upcomingBookings, completedTests, 0);
    }

    public async Task<SpecialistDashboardResponse> GetSpecialistDashboardAsync(Guid userId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(userId);
        if (specialist == null) return new SpecialistDashboardResponse(0, 0, 0, 0, new(), new(), new());

        return await _dashboardRepo.GetSpecialistDashboardDataAsync(specialist.SpecialistId);
    }
}
