namespace CoinApp.Application.Common.Options;

public sealed class EmailVerificationOptions
{
    public int CodeExpirationMinutes { get; init; } = 15;
    public bool ExposeCodeInResponse { get; init; }
}
