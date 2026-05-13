namespace CoinApp.Application.Dtos.SupportTickets;

public sealed record SupportTicketMessageDto(
    Guid Id,
    string Message,
    bool IsStaff,
    DateTime CreatedAtUtc);
