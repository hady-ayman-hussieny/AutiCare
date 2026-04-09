using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponse> CreateAsync(Guid parentUserId, CreateBookingRequest request);
    Task<BookingResponse?> GetByIdAsync(int bookingId, Guid userId, string role);
    Task<IEnumerable<BookingResponse>> GetMyBookingsAsync(Guid userId, string role);
    Task<IEnumerable<BookingResponse>> GetUpcomingBookingsAsync(Guid specialistUserId);
    Task UpdateStatusAsync(int bookingId, string status, Guid userId, string role);
    Task UpdateAsync(int bookingId, UpdateBookingRequest request, Guid userId, string role);
    Task DeleteAsync(int bookingId, Guid parentUserId);
}
