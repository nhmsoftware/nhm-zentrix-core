namespace CoinApp.Application.Dtos.Auth;

public sealed class ResendEmailVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}
