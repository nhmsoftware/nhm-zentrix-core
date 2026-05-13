namespace CoinApp.Application.Dtos.SupportTickets;

public sealed record SupportTicketDto(
    Guid Id,
    string Type,
    int TypeValue,
    string Priority,
    int PriorityValue,
    string Status,
    int StatusValue,
    string Message,
    DateTime CreatedAtUtc);
