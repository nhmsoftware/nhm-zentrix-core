namespace CoinApp.Domain.Entities;

public sealed class PasswordResetCode : BaseEntity
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public string? ResetTokenHash { get; set; }
    public DateTime? ResetTokenExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
}
