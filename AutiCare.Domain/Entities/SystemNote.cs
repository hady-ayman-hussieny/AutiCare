using System;
using System.ComponentModel.DataAnnotations;

namespace AutiCare.Domain.Entities;

public class SystemNote
{
    [Key] public int NoteId { get; set; }
    public int SpecialistId { get; set; }
    public int? ChildId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Specialist Specialist { get; set; } = null!;
    public Child? Child { get; set; }
}
