using CoinApp.Application.Dtos.Auth;
using FluentValidation;

namespace CoinApp.Application.Validators.Auth;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.ResetToken)
            .NotEmpty()
            .WithMessage("validation.auth_reset_token_required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("validation.auth_password_required")
            .MinimumLength(8)
            .WithMessage("validation.auth_password_min_length")
            .MaximumLength(128)
            .WithMessage("validation.auth_password_max_length");
    }
}
