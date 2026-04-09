using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace AutiCare.Domain.Entities;

public class Attachment
{
    [Key] public int AttachmentId { get; set; }
    public int ChildId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public Child Child { get; set; } = null!;
}
