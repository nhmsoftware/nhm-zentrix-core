namespace CoinApp.Application.Dtos.Auth;

public sealed record RegisterResponseDto(
    AuthUserDto User,
    bool EmailVerificationRequired,
    DateTime VerificationCodeExpiresAtUtc,
    string? EmailVerificationCode);
