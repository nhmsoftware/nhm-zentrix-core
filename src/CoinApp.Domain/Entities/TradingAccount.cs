using CoinApp.Domain.Enums;

namespace CoinApp.Domain.Entities;

public sealed class TradingAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public TradingAccountType Type { get; set; } = TradingAccountType.Real;
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public bool ActiveProtectCost { get; set; }
}
