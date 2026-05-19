namespace CoinApp.Application.Dtos.Auth;

public sealed class ResetPasswordRequest
{
    public string ResetToken { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
