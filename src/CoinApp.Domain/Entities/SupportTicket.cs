using CoinApp.Domain.Enums;

namespace CoinApp.Domain.Entities;

public sealed class SupportTicket : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public SupportTicketType Type { get; set; }
    public SupportTicketPriority Priority { get; set; }
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;
    public string Message { get; set; } = string.Empty;
    public ICollection<SupportTicketMessage> Messages { get; set; } = new List<SupportTicketMessage>();
}
