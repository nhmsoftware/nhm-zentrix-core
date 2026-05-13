namespace CoinApp.Application.Dtos.Auth;

public sealed record AuthUserDto(
    Guid Id,
    string FullName,
    string Email,
    bool IsActive);

