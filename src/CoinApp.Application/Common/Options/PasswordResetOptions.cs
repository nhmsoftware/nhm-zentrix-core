namespace CoinApp.Application.Common.Options;

public sealed class PasswordResetOptions
{
    public int CodeExpirationMinutes { get; init; } = 15;
    public int ResetTokenExpirationMinutes { get; init; } = 15;
    public int MaxVerifyAttempts { get; init; } = 5;
    public bool ExposeCodeInResponse { get; init; }
}
