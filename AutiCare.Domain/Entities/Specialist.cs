using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Specialist
{
    [Key] public int SpecialistId { get; set; }
    public Guid UserId { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? NationalId { get; set; }
    public string? Gender { get; set; }
    public string? Specialization { get; set; }
    public int YearsExperience { get; set; } = 0;
    public string? LicenseNumber { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<TreatmentPlan> TreatmentPlans { get; set; } = new List<TreatmentPlan>();
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
}
