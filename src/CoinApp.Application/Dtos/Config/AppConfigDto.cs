namespace CoinApp.Application.Dtos.Config;

public sealed record AppConfigDto(
    string Key,
    string Value,
    string? Description);
