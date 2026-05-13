namespace CoinApp.Application.Dtos.Auth;

public sealed record AuthResponseDto(
    AuthUserDto User,
    string AccessToken,
    DateTime ExpiresAtUtc);

