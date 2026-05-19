namespace CoinApp.Application.Dtos.Auth;

public sealed record VerifyResetCodeResponseDto(
    string Email,
    string ResetToken,
    DateTime ResetTokenExpiresAtUtc);
