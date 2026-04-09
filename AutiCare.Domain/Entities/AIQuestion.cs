using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class AIQuestion
{
   
    [Key] public int QuestionId { get; set; }
    public int TestId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int QuestionOrder { get; set; }

    public Test Test { get; set; } = null!;
    public ICollection<ParentAnswer> Answers { get; set; } = new List<ParentAnswer>();
}
