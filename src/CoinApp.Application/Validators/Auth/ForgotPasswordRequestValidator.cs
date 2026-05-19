using CoinApp.Application.Dtos.Auth;
using FluentValidation;

namespace CoinApp.Application.Validators.Auth;

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("validation.auth_email_required")
            .MaximumLength(200)
            .WithMessage("validation.auth_email_length")
            .EmailAddress()
            .WithMessage("validation.auth_email_invalid");
    }
}
