using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class AIResult
{
    [Key] public int AIResultId { get; set; }
    public int ParentTestId { get; set; }
    public decimal Score { get; set; }
    public string StatusLevel { get; set; } = "Pending";
    public string? Recommendation { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public ParentTest ParentTest { get; set; } = null!;
}
