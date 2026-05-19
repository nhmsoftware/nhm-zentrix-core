using CoinApp.Application.Dtos.Auth;
using FluentValidation;

namespace CoinApp.Application.Validators.Auth;

public sealed class VerifyResetCodeRequestValidator : AbstractValidator<VerifyResetCodeRequest>
{
    public VerifyResetCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("validation.auth_email_required")
            .MaximumLength(200)
            .WithMessage("validation.auth_email_length")
            .EmailAddress()
            .WithMessage("validation.auth_email_invalid");

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("validation.auth_reset_code_required")
            .Length(6)
            .WithMessage("validation.auth_reset_code_length")
            .Matches("^[0-9]{6}$")
            .WithMessage("validation.auth_reset_code_format");
    }
}
