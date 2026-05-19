namespace CoinApp.Application.Dtos.Auth;

public sealed record EmailVerificationResponseDto(
    string Email,
    bool EmailVerified,
    DateTime? EmailVerifiedAtUtc,
    DateTime? VerificationCodeExpiresAtUtc,
    string? EmailVerificationCode);
