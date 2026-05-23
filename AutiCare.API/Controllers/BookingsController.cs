using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutiCare.API.Controllers;

/// <summary>
/// Manages booking requests between Parents and Specialists.
/// Flow: Parent creates (Pending) → Specialist confirms/rejects → Chat/Zoom session.
/// </summary>
[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : BaseController
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>Creates a booking request. Status defaults to Pending.</summary>
    [HttpPost]
    [Authorize(Roles = "Parent")]
    [ProducesResponseType(typeof(BookingResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var booking = await _bookingService.CreateAsync(GetUserId(), request);
            return CreatedAtAction(nameof(GetById), new { id = booking.BookingId }, booking);
        }
        catch (KeyNotFoundException ex)   { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    /// <summary>Gets a single booking by ID (Parent or Specialist must own it).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookingResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id, GetUserId(), GetUserRole());
        return booking == null ? NotFound(new { error = "Booking not found." }) : Ok(booking);
    }

    /// <summary>Gets all bookings for the authenticated user (Parent sees own, Specialist sees assigned).</summary>
    [HttpGet("my-bookings")]
    [ProducesResponseType(typeof(BookingResponse[]), 200)]
    public async Task<IActionResult> GetMyBookings()
    {
        var bookings = await _bookingService.GetMyBookingsAsync(GetUserId(), GetUserRole());
        return Ok(bookings);
    }

    /// <summary>Gets upcoming confirmed bookings for the authenticated Specialist.</summary>
    [HttpGet("upcoming")]
    [Authorize(Roles = "Doctor,Therapist")]
    [ProducesResponseType(typeof(BookingResponse[]), 200)]
    public async Task<IActionResult> GetUpcomingBookings()
    {
        var bookings = await _bookingService.GetUpcomingBookingsAsync(GetUserId());
        return Ok(bookings);
    }

    /// <summary>Updates booking date/time/reason (Parent only).</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingRequest request)
    {
        try
        {
            await _bookingService.UpdateAsync(id, request, GetUserId(), GetUserRole());
            return Ok(new { message = "Booking updated successfully." });
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    /// <summary>
    /// Updates booking status.
    /// Specialist can set: Confirmed, Rejected, Completed.
    /// Parent can set: Cancelled.
    /// Body: { "status": "Confirmed" }
    /// </summary>
    [HttpPatch("{id}/status")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateBookingStatusRequest request)
    {
        try
        {
            await _bookingService.UpdateStatusAsync(id, request.Status, GetUserId(), GetUserRole());
            return Ok(new { message = $"Booking marked as {request.Status}." });
        }
        catch (ArgumentException ex)           { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }

    /// <summary>Soft-deletes a booking (Parent only, own bookings).</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Parent")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _bookingService.DeleteAsync(id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex)        { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
    }
}
