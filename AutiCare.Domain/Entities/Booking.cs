using System;
using System.ComponentModel.DataAnnotations;

namespace AutiCare.Domain.Entities;

public class Booking
{
    [Key] public int BookingId { get; set; }
    public int ParentId { get; set; }
    public int SpecialistId { get; set; }
    public int? ChildId { get; set; }
    
    public DateTime BookingDate { get; set; }
    public TimeSpan? BookingTime { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Parent Parent { get; set; } = null!;
    public Specialist Specialist { get; set; } = null!;
    public Child? Child { get; set; }
}
