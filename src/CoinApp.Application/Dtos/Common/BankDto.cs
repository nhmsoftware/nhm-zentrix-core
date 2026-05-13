namespace CoinApp.Application.Dtos.Common;

public sealed record BankDto(
    Guid Id,
    string Bin,
    string Code,
    string Name,
    string ShortName);
