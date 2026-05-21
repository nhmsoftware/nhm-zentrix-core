using CoinApp.Application.Dtos.Auth;
using FluentValidation;

namespace CoinApp.Application.Validators.Auth;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithMessage("validation.auth_current_password_required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithMessage("validation.auth_new_password_required")
            .MinimumLength(8)
            .WithMessage("validation.auth_password_min_length")
            .MaximumLength(128)
            .WithMessage("validation.auth_password_max_length")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("validation.auth_new_password_must_be_different");
    }
}
