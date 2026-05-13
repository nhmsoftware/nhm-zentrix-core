namespace CoinApp.Application.Dtos.Profile;

public sealed record BankAccountDto(
    string BinBank,
    string AccountBank,
    string AccountBankName);
