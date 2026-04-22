using AutiCare.Application.DTOs;
using FluentValidation;
using System.Linq;
using System.Text.RegularExpressions;

namespace AutiCare.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().WithMessage("Full Name is required.").MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number.");
        RuleFor(x => x.Phone)
            .Must(p => !string.IsNullOrEmpty(p) && Regex.IsMatch(p, @"^01[0125][0-9]{8}$"))
            .WithMessage("Please enter a valid Egyptian mobile number.");

        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Parent" or "Doctor" or "Therapist")
            .WithMessage("Role must be Parent, Doctor, or Therapist");
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Phone)
            .Must(p => string.IsNullOrEmpty(p) || Regex.IsMatch(p, @"^01[0125][0-9]{8}$"))
            .WithMessage("Please enter a valid Egyptian mobile number.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.").EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required.");
    }
}

public class CreateChildRequestValidator : AbstractValidator<CreateChildRequest>
{
    public CreateChildRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("First Name is required.").MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Last Name is required.").MaximumLength(100);
        RuleFor(x => x.DateOfBirth).NotEmpty().WithMessage("Date of Birth is required.")
            .LessThan(DateTime.Today).WithMessage("Date of birth must be in the past");
        RuleFor(x => x.Gender).NotEmpty().WithMessage("Gender is required.")
            .Must(g => g is "Male" or "Female")
            .WithMessage("Gender must be Male or Female");
    }
}

public class CreateTreatmentPlanValidator : AbstractValidator<CreateTreatmentPlanRequest>
{
    public CreateTreatmentPlanValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0);
        RuleFor(x => x.SpecialistId).GreaterThan(0);
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("End date must be after start date");
    }
}

public class StartScreeningRequestValidator : AbstractValidator<StartScreeningRequest>
{
    public StartScreeningRequestValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0).WithMessage("ChildId must be a positive integer.");
    }
}

public class SubmitScreeningRequestValidator : AbstractValidator<SubmitScreeningRequest>
{
    public SubmitScreeningRequestValidator()
    {
        RuleFor(x => x.ChildId).GreaterThan(0).WithMessage("childId required and must be positive.");
        RuleFor(x => x.Answers).NotNull().WithMessage("answers required.")
            .Must(a => a != null && a.Count == 10)
            .WithMessage("complete questionnaire required (Exactly 10 answers).");
        RuleForEach(x => x.Answers).ChildRules(a =>
        {
            a.RuleFor(x => x.QuestionId).InclusiveBetween(1, 10)
                .WithMessage("QuestionId must be between 1 and 10.");
            a.RuleFor(x => x.AnswerValue).InclusiveBetween(0, 1)
                .WithMessage("AnswerValue must be 0 (No) or 1 (Yes).");
        });
    }
}

public class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(x => x.SpecialistId).GreaterThan(0).WithMessage("SpecialistId is required.");
        RuleFor(x => x.BookingDate).NotEmpty().WithMessage("Booking Date is required.")
            .Must(d => d.Date >= DateTime.Today).WithMessage("Booking Date cannot be in the past.");
        RuleFor(x => x.BookingTime).NotEmpty().WithMessage("Booking Time is required.");
    }
}
