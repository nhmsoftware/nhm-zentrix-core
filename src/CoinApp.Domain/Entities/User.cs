using CoinApp.Domain.Enums;

namespace CoinApp.Domain.Entities;

public sealed class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public UserGender Gender { get; set; } = UserGender.Unspecified;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public AccountVerificationStatus VerificationStatus { get; set; } = AccountVerificationStatus.Unverified;
    public string ReferralCode { get; set; } = string.Empty;
    public decimal MoneyBalance { get; set; }
    public string? BinBank { get; set; }
    public string? AccountBank { get; set; }
    public string? AccountBankName { get; set; }
    public string? IdentityFrontImagePath { get; set; }
    public string? IdentityBackImagePath { get; set; }
}
