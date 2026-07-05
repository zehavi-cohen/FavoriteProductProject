using backend.DTOs.Auth;
using FluentValidation;

namespace backend.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty()
            .WithMessage("יש להזין שם משתמש או אימייל")
            .MaximumLength(100)
            .WithMessage("שם משתמש או אימייל ארוך מדי");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("יש להזין סיסמה")
            .MaximumLength(100)
            .WithMessage("סיסמה ארוכה מדי");
    }
}