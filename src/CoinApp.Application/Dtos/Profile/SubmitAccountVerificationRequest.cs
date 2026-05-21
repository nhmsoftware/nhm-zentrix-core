using CoinApp.Application.Common.Storage;

namespace CoinApp.Application.Dtos.Profile;

public sealed class SubmitAccountVerificationRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public int Gender { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string BinBank { get; set; } = string.Empty;
    public string AccountBank { get; set; } = string.Empty;
    public string AccountBankName { get; set; } = string.Empty;
    public FileUpload? CccdFrontImage { get; set; }
    public FileUpload? CccdBackImage { get; set; }
}
