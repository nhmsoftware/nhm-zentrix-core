namespace CoinApp.Application.Dtos.SupportTickets;

public sealed record SupportTicketThreadDto(
    SupportTicketDto Ticket,
    IReadOnlyList<SupportTicketMessageDto> Messages);
