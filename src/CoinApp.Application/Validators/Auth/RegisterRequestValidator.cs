using CoinApp.Application.Dtos.Auth;
using FluentValidation;

namespace CoinApp.Application.Validators.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("validation.auth_full_name_required")
            .MaximumLength(100)
            .WithMessage("validation.auth_full_name_length");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("validation.auth_email_required")
            .MaximumLength(200)
            .WithMessage("validation.auth_email_length")
            .EmailAddress()
            .WithMessage("validation.auth_email_invalid");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("validation.auth_password_required")
            .MinimumLength(8)
            .WithMessage("validation.auth_password_min_length")
            .MaximumLength(128)
            .WithMessage("validation.auth_password_max_length");

        RuleFor(x => x.ReferralCode)
            .MaximumLength(32)
            .WithMessage("validation.auth_referral_code_length")
            .Matches("^[A-Za-z0-9]*$")
            .WithMessage("validation.auth_referral_code_format")
            .When(x => !string.IsNullOrWhiteSpace(x.ReferralCode));
    }
}
