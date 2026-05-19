namespace CoinApp.Domain.Entities;

public sealed class EmailVerificationCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
}
