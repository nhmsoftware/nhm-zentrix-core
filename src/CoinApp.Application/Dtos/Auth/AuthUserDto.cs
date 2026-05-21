namespace CoinApp.Application.Dtos.Auth;

public sealed record AuthUserDto(
    Guid Id,
    string FullName,
    string Email,
    string AdminRole,
    bool IsActive,
    bool EmailVerified);
