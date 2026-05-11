namespace CoinApp.Application.Dtos.Market;

public sealed record CoinDto(
    Guid Id,
    string Symbol,
    string Name,
    int Decimals,
    bool IsActive);

