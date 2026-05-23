using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Notification
{
    [Key] public int NotificationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
