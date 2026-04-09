using Microsoft.AspNetCore.Identity;

namespace AutiCare.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // "Parent", "Doctor", "Therapist"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
}
