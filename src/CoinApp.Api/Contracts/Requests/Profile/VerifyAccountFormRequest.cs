using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Contracts.Requests.Profile;

public sealed class VerifyAccountFormRequest
{
    [FromForm(Name = "first_name")]
    public string FirstName { get; set; } = string.Empty;

    [FromForm(Name = "last_name")]
    public string LastName { get; set; } = string.Empty;

    [FromForm(Name = "dob")]
    public DateOnly Dob { get; set; }

    [FromForm(Name = "gender")]
    public string Gender { get; set; } = string.Empty;

    [FromForm(Name = "phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [FromForm(Name = "address")]
    public string Address { get; set; } = string.Empty;

    [FromForm(Name = "bin_bank")]
    public string BinBank { get; set; } = string.Empty;

    [FromForm(Name = "account_bank")]
    public string AccountBank { get; set; } = string.Empty;

    [FromForm(Name = "account_bank_name")]
    public string AccountBankName { get; set; } = string.Empty;

    [FromForm(Name = "cccd_front_image")]
    public IFormFile? CccdFrontImage { get; set; }

    [FromForm(Name = "cccd_back_image")]
    public IFormFile? CccdBackImage { get; set; }
}
