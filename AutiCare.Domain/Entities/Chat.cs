using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Chat
{
    [Key] public int ChatId { get; set; }
    public int ParentId { get; set; }
    public int SpecialistId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Parent Parent { get; set; } = null!;
    public Specialist Specialist { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
