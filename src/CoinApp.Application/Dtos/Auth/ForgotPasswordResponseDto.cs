namespace CoinApp.Application.Dtos.Auth;

public sealed record ForgotPasswordResponseDto(
    string Email,
    DateTime? ResetCodeExpiresAtUtc,
    string? ResetCode);
