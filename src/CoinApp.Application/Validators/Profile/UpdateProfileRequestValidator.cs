using CoinApp.Application.Common.Constants;
using CoinApp.Application.Dtos.Profile;
using FluentValidation;

namespace CoinApp.Application.Validators.Profile;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("validation.profile_first_name_required")
            .MaximumLength(100)
            .WithMessage("validation.profile_first_name_length");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("validation.profile_last_name_required")
            .MaximumLength(100)
            .WithMessage("validation.profile_last_name_length");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .WithMessage("validation.profile_date_of_birth_required")
            .Must(date => date <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("validation.profile_date_of_birth_invalid");

        RuleFor(x => x.Gender)
            .NotEqual(UserGenderValues.Unspecified)
            .WithMessage("validation.profile_gender_required")
            .Must(IsSupportedGender)
            .WithMessage("validation.profile_gender_invalid");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("validation.profile_phone_number_required")
            .MaximumLength(20)
            .WithMessage("validation.profile_phone_number_length");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("validation.profile_address_required")
            .MaximumLength(500)
            .WithMessage("validation.profile_address_length");
    }

    private static bool IsSupportedGender(int value)
    {
        return value is UserGenderValues.Male or UserGenderValues.Female;
    }
}
