using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Message
{
    [Key] public int MessageId { get; set; }
    public int ChatId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderType { get; set; } = string.Empty;
    public string SenderUserId { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public Chat Chat { get; set; } = null!;
}
