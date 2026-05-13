namespace CoinApp.Application.Dtos.Auth;

public sealed record AccessTokenDto(
    string AccessToken,
    DateTime ExpiresAtUtc);

