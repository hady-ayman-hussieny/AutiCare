using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class ParentAnswer
{
     [Key] public int AnswerId { get; set; }
    public int ParentTestId { get; set; }
    public int QuestionId { get; set; }
    public int AnswerValue { get; set; }
    public string? AnswerText { get; set; }
    public DateTime AnswerDate { get; set; } = DateTime.UtcNow;

    public ParentTest ParentTest { get; set; } = null!;
    public AIQuestion Question { get; set; } = null!;
}
