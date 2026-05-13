namespace CoinApp.Application.Dtos.Profile;

public sealed record UserProfileDto(
    Guid Id,
    string FullName,
    string Email,
    string? FirstName,
    string? LastName,
    DateOnly? DateOfBirth,
    string Gender,
    int GenderValue,
    string? PhoneNumber,
    string? Address,
    decimal Money,
    string VerificationStatus,
    int VerificationStatusValue,
    string ReferralCode,
    BankAccountDto? Bank,
    bool IsActive);
