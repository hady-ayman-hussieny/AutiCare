using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Session
{
    [Key] public int SessionId { get; set; }
    public int TreatmentId { get; set; }
    public string? Report { get; set; }
    public string? ActivityNotes { get; set; }
    public string? SessionNotes { get; set; }
    public DateTime SessionDate { get; set; }
    public TimeSpan? SessionTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public TreatmentPlan TreatmentPlan { get; set; } = null!;
}
