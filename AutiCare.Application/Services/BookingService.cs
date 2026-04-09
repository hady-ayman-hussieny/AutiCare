using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IParentRepository _parentRepo;
    private readonly IDoctorRepository _specialistRepo;
    private readonly IChildRepository _childRepo;

    public BookingService(
        IBookingRepository bookingRepo, 
        IParentRepository parentRepo, 
        IDoctorRepository specialistRepo,
        IChildRepository childRepo)
    {
        _bookingRepo = bookingRepo;
        _parentRepo = parentRepo;
        _specialistRepo = specialistRepo;
        _childRepo = childRepo;
    }

    public async Task<BookingResponse> CreateAsync(Guid parentUserId, CreateBookingRequest request)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        // IDOR Fix: Validate child belongs to the parent
        if (request.ChildId.HasValue)
        {
            var child = await _childRepo.GetByIdAsync(request.ChildId.Value);
            if (child == null || child.ParentId != parent.ParentId)
                throw new UnauthorizedAccessException("You are not authorized to create a booking for this child.");
        }

        var booking = new Booking
        {
            ParentId = parent.ParentId,
            SpecialistId = request.SpecialistId,
            ChildId = request.ChildId,
            BookingDate = request.BookingDate,
            BookingTime = request.BookingTime,
            Reason = request.Reason,
            Status = "Pending"
        };

        await _bookingRepo.AddAsync(booking);
        await _bookingRepo.SaveChangesAsync();
        
        var addedBooking = await _bookingRepo.GetByIdAsync(booking.BookingId);
        return ToResponse(addedBooking!);
    }

    public async Task<BookingResponse?> GetByIdAsync(int bookingId, Guid userId, string role)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking == null) return null;

        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || booking.ParentId != parent.ParentId) return null;
        }
        else if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _specialistRepo.GetByUserIdAsync(userId);
            if (specialist == null || booking.SpecialistId != specialist.SpecialistId) return null;
        }

        return ToResponse(booking);
    }

    public async Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(Guid userId, string role)
    {
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null) return Enumerable.Empty<BookingResponse>();
            var list = await _bookingRepo.GetByParentIdAsync(parent.ParentId);
            return list.Select(ToResponse);
        }
        else if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _specialistRepo.GetByUserIdAsync(userId);
            if (specialist == null) return Enumerable.Empty<BookingResponse>();
            var list = await _bookingRepo.GetBySpecialistIdAsync(specialist.SpecialistId);
            return list.Select(ToResponse);
        }
        
        return Enumerable.Empty<BookingResponse>();
    }

    public async Task<IEnumerable<BookingResponse>> GetUpcomingBookingsAsync(Guid specialistUserId)
    {
        var specialist = await _specialistRepo.GetByUserIdAsync(specialistUserId);
        if (specialist == null) return Enumerable.Empty<BookingResponse>();
        var list = await _bookingRepo.GetUpcomingBySpecialistIdAsync(specialist.SpecialistId);
        return list.Select(ToResponse);
    }

    public async Task UpdateStatusAsync(int bookingId, string status, Guid userId, string role)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) ?? throw new KeyNotFoundException("Booking not found");
        
        if (role == "Doctor" || role == "Therapist")
        {
            var specialist = await _specialistRepo.GetByUserIdAsync(userId);
            if (specialist == null || booking.SpecialistId != specialist.SpecialistId) throw new UnauthorizedAccessException();
        }
        else if (role == "Parent" && status == "Cancelled")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || booking.ParentId != parent.ParentId) throw new UnauthorizedAccessException();
        }
        else
        {
            throw new UnauthorizedAccessException("Cannot update this status");
        }

        booking.Status = status;
        _bookingRepo.Update(booking);
        await _bookingRepo.SaveChangesAsync();
    }

    public async Task UpdateAsync(int bookingId, UpdateBookingRequest request, Guid userId, string role)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) ?? throw new KeyNotFoundException("Booking not found");

        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || booking.ParentId != parent.ParentId) throw new UnauthorizedAccessException();
        }

        if (request.BookingDate.HasValue) booking.BookingDate = request.BookingDate.Value;
        if (request.BookingTime.HasValue) booking.BookingTime = request.BookingTime;
        if (request.Reason != null) booking.Reason = request.Reason;
        if (request.Status != null && (role == "Doctor" || role == "Therapist")) booking.Status = request.Status;

        _bookingRepo.Update(booking);
        await _bookingRepo.SaveChangesAsync();
    }

    public async Task DeleteAsync(int bookingId, Guid userId)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) ?? throw new KeyNotFoundException("Booking not found");
        var parent = await _parentRepo.GetByUserIdAsync(userId);
        if (parent == null || booking.ParentId != parent.ParentId) throw new UnauthorizedAccessException();

        booking.IsDeleted = true;
        booking.DeletedAt = DateTime.UtcNow;
        booking.DeletedBy = userId.ToString();
        _bookingRepo.Update(booking);
        await _bookingRepo.SaveChangesAsync();
    }

    private static BookingResponse ToResponse(Booking b) => new(
        b.BookingId,
        b.ParentId, b.Parent?.Name ?? "",
        b.SpecialistId, b.Specialist?.Name ?? "",
        b.ChildId,
        b.Child != null ? $"{b.Child.FirstName} {b.Child.LastName}" : null,
        b.BookingDate, b.BookingTime,
        b.Status, b.Reason,
        b.CreatedAt
    );
}
