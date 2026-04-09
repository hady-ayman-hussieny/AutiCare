using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Test
{
    [Key] public int TestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AIQuestion> AIQuestions { get; set; } = new List<AIQuestion>();
    public ICollection<ParentTest> ParentTests { get; set; } = new List<ParentTest>();
}
