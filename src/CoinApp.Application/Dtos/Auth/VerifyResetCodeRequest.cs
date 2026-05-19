namespace CoinApp.Application.Dtos.Auth;

public sealed class VerifyResetCodeRequest
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
