using System.Text.Json.Serialization;

namespace CoinApp.Application.Dtos.Accounts;

public sealed class ToggleProtectAccountRequest
{
    [JsonPropertyName("account_id")]
    public Guid AccountId { get; set; }
}
