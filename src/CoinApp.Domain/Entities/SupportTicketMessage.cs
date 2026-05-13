namespace CoinApp.Domain.Entities;

public sealed class SupportTicketMessage : BaseEntity
{
    public Guid SupportTicketId { get; set; }
    public SupportTicket SupportTicket { get; set; } = null!;
    public Guid? SenderUserId { get; set; }
    public User? SenderUser { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsStaff { get; set; }
}
