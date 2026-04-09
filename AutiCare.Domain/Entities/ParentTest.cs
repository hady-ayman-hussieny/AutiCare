using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class ParentTest
{
    [Key] public int ParentTestId { get; set; }
    public int ParentId { get; set; }
    public int ChildId { get; set; }
    public int TestId { get; set; }
    public DateTime TestDate { get; set; } = DateTime.UtcNow;
    public bool IsCompleted { get; set; } = false;

    public Parent Parent { get; set; } = null!;
    public Child Child { get; set; } = null!;
    public Test Test { get; set; } = null!;
    public ICollection<ParentAnswer> Answers { get; set; } = new List<ParentAnswer>();
    public AIResult? AIResult { get; set; }
   
}
