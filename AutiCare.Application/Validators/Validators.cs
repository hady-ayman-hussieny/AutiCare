using AutiCare.Application.DTOs;
using FluentValidation;

namespace AutiCare.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Parent" or "Doctor" or "Therapist")
            .WithMessage("Role must be Parent, Doctor, or Therapist");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class CreateChildRequestValidator : AbstractValidator<CreateChildRequest>
{
    public CreateChildRequestValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DateOfBirth).LessThan(DateTime.Today).WithMessage("Date of birth must be in the past");
        RuleFor(x => x.Gender).NotEmpty().Must(g => g is "Male" or "Female")
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

public class SubmitAnswersValidator : AbstractValidator<SubmitAnswersRequest>
{
    public SubmitAnswersValidator()
    {
        RuleFor(x => x.ParentTestId).GreaterThan(0);
        RuleFor(x => x.Answers).NotEmpty().Must(a => a.Count == 20)
            .WithMessage("Exactly 20 answers are required");
        RuleForEach(x => x.Answers).ChildRules(a =>
        {
            a.RuleFor(x => x.AnswerValue).InclusiveBetween(0, 2)
                .WithMessage("Answer value must be between 0 and 2");
        });
    }
}
