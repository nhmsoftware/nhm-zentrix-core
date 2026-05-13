using CoinApp.Domain.Enums;

namespace CoinApp.Domain.Entities;

public sealed class WalletTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public WalletTransactionType Type { get; set; }
    public WalletTransactionStatus Status { get; set; } = WalletTransactionStatus.Pending;
    public decimal Money { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Note { get; set; }
}
