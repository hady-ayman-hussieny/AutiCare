using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

[Route("api/bookings")]
[Authorize]
public class BookingsController : BaseController
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var booking = await _bookingService.CreateAsync(GetUserId(), request);
        return CreatedAtAction(nameof(GetById), new { id = booking.BookingId }, booking);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id, GetUserId(), GetUserRole());
        return booking == null ? NotFound() : Ok(booking);
    }

    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings()
    {
        var bookings = await _bookingService.GetMyBookingsAsync(GetUserId(), GetUserRole());
        return Ok(bookings);
    }

    [HttpGet("upcoming")]
    [Authorize(Roles = "Doctor,Therapist")]
    public async Task<IActionResult> GetUpcomingBookings()
    {
        var bookings = await _bookingService.GetUpcomingBookingsAsync(GetUserId());
        return Ok(bookings);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateBookingRequest request)
    {
        await _bookingService.UpdateAsync(id, request, GetUserId(), GetUserRole());
        return Ok(new { message = "Booking updated successfully" });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        await _bookingService.UpdateStatusAsync(id, status, GetUserId(), GetUserRole());
        return Ok(new { message = $"Booking marked as {status}" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> Delete(int id)
    {
        await _bookingService.DeleteAsync(id, GetUserId());
        return NoContent();
    }
}
