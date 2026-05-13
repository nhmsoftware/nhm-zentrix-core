using CoinApp.Application.Dtos.Auth;
using FluentValidation;

namespace CoinApp.Application.Validators.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("validation.auth_email_required")
            .MaximumLength(200)
            .WithMessage("validation.auth_email_length")
            .EmailAddress()
            .WithMessage("validation.auth_email_invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("validation.auth_password_required");
    }
}

