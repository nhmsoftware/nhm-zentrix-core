namespace CoinApp.Application.Dtos.SupportTickets;

public sealed class CreateSupportTicketRequest
{
    public int Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public int Priority { get; set; }
}
