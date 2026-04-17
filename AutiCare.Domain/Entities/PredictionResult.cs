using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutiCare.Domain.Entities;

public class PredictionResult
{
    [Key] 
    public int Id { get; set; }
    
    public int ChildId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    public string PredictionClass { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,4)")]
    public decimal? ConfidenceScore { get; set; }
    
    [Required]
    public string RawResponse { get; set; } = string.Empty;

    public Child Child { get; set; } = null!;
}
