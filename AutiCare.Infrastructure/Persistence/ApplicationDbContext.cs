using AutiCare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Specialist> Specialists => Set<Specialist>();
    public DbSet<Child> Children => Set<Child>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<AIQuestion> AIQuestions => Set<AIQuestion>();
    public DbSet<ParentTest> ParentTests => Set<ParentTest>();
    public DbSet<ParentAnswer> ParentAnswers => Set<ParentAnswer>();
    public DbSet<AIResult> AIResults => Set<AIResult>();
    public DbSet<PredictionResult> PredictionResults => Set<PredictionResult>();
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<SystemNote> SystemNotes => Set<SystemNote>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ParentAnswer>()
    .HasOne(pa => pa.ParentTest)
    .WithMany(pt => pt.Answers)
    .HasForeignKey(pa => pa.ParentTestId)
    .OnDelete(DeleteBehavior.NoAction);

        // Table names matching SQL script
        builder.Entity<Specialist>().ToTable("Specialist");
        builder.Entity<AIResult>().ToTable("AIResults");
        builder.Entity<PredictionResult>().ToTable("PredictionResults");
        builder.Entity<AIQuestion>().ToTable("AIQuestions");
        builder.Entity<ParentTest>().ToTable("ParentTest");
        builder.Entity<ParentAnswer>().ToTable("ParentAnswers");

        // Notification.Message column name conflict fix
        builder.Entity<Notification>()
            .Property(n => n.MessageText).HasColumnName("Message");

        // Unique constraints
        builder.Entity<AIResult>()
            .HasIndex(r => r.ParentTestId).IsUnique();
        builder.Entity<ParentAnswer>()
            .HasIndex(a => new { a.ParentTestId, a.QuestionId }).IsUnique();

        // Relationships
        builder.Entity<Child>()
            .HasOne(c => c.Parent).WithMany(p => p.Children)
            .HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PredictionResult>()
            .HasOne(pr => pr.Child)
            .WithMany(c => c.PredictionResults)
            .HasForeignKey(pr => pr.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TreatmentPlan>()
            .HasOne(tp => tp.Child).WithMany(c => c.TreatmentPlans)
            .HasForeignKey(tp => tp.ChildId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<TreatmentPlan>()
            .HasOne(tp => tp.Specialist).WithMany(s => s.TreatmentPlans)
            .HasForeignKey(tp => tp.SpecialistId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Session>()
            .HasOne(s => s.TreatmentPlan).WithMany(tp => tp.Sessions)
            .HasForeignKey(s => s.TreatmentId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ParentTest>()
            .HasOne(pt => pt.AIResult).WithOne(ar => ar.ParentTest)
            .HasForeignKey<AIResult>(ar => ar.ParentTestId);

        builder.Entity<Chat>()
            .HasOne(c => c.Parent).WithMany(p => p.Chats)
            .HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Chat>()
            .HasOne(c => c.Specialist).WithMany(s => s.Chats)
            .HasForeignKey(c => c.SpecialistId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Chat).WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId);

        builder.Entity<Booking>()
            .HasOne(b => b.Parent)
            .WithMany()
            .HasForeignKey(b => b.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Specialist)
            .WithMany()
            .HasForeignKey(b => b.SpecialistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Child)
            .WithMany()
            .HasForeignKey(b => b.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SystemNote>()
            .HasOne(s => s.Specialist)
            .WithMany()
            .HasForeignKey(s => s.SpecialistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SystemNote>()
            .HasOne(s => s.Child)
            .WithMany()
            .HasForeignKey(s => s.ChildId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed roles
        var parentRoleId = new Guid("8d60233e-1081-426b-a19f-0740a33118f6");
        var doctorRoleId = new Guid("63840e53-43ce-4348-9336-67056cf98858");
        var therapistRoleId = new Guid("4678d46a-77c8-47f9-8d76-e3d6796c810d");

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = parentRoleId, Name = "Parent", NormalizedName = "PARENT" },
            new IdentityRole<Guid> { Id = doctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" },
            new IdentityRole<Guid> { Id = therapistRoleId, Name = "Therapist", NormalizedName = "THERAPIST" }
        );
    }
}
