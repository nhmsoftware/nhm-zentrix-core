namespace CoinApp.Application.Dtos.Accounts;

public sealed record TradingAccountDto(
    Guid Id,
    string Type,
    int TypeValue,
    string AccountNumber,
    string Name,
    decimal Balance,
    bool ActiveProtectCost);
