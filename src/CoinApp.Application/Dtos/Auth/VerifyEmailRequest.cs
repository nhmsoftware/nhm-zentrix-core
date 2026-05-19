namespace CoinApp.Application.Dtos.Auth;

public sealed class VerifyEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}
