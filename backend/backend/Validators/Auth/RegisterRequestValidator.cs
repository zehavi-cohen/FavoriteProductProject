using backend.DTOs.Auth;
using FluentValidation;

namespace backend.Validators.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("יש להזין שם משתמש")
            .MaximumLength(50)
            .WithMessage("שם משתמש ארוך מדי");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("יש להזין אימייל")
            .EmailAddress()
            .WithMessage("כתובת אימייל לא תקינה")
            .MaximumLength(100)
            .WithMessage("אימייל ארוך מדי");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("יש להזין סיסמה")
            .MinimumLength(6)
            .WithMessage("סיסמה חייבת להכיל לפחות 6 תווים")
            .MaximumLength(100)
            .WithMessage("סיסמה ארוכה מדי");
    }
}