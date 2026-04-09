using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Parent
{
    [Key] public int ParentId { get; set; }
    public Guid UserId { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? NationalId { get; set; }
    public string? ProfilePictureJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<Child> Children { get; set; } = new List<Child>();
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
}
