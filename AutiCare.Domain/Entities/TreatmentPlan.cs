using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class TreatmentPlan
{
    [Key] public int TreatmentId { get; set; }
    public int ChildId { get; set; }
    public int SpecialistId { get; set; }
    public string? Notes { get; set; }
    public string? Progress { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Goal { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Child Child { get; set; } = null!;
    public Specialist Specialist { get; set; } = null!;
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
