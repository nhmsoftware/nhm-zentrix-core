namespace CoinApp.Application.Dtos.Wallet;

public sealed record WalletTransactionDto(
    Guid Id,
    string Type,
    int TypeValue,
    string Status,
    int StatusValue,
    decimal Money,
    decimal BalanceBefore,
    decimal BalanceAfter,
    string? Note,
    DateTime CreatedAtUtc);
