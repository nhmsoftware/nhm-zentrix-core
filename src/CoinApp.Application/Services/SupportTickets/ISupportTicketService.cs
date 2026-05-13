using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.SupportTickets;

namespace CoinApp.Application.Services.SupportTickets;

public interface ISupportTicketService
{
    Task<ServiceResult<PaginatedResult<SupportTicketDto>>> GetCurrentUserTicketsAsync(int page, string? keyword, int? status, CancellationToken cancellationToken = default);
    Task<ServiceResult<SupportTicketDto>> CreateAsync(CreateSupportTicketRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<SupportTicketThreadDto>> GetThreadAsync(Guid ticketId, int page, CancellationToken cancellationToken = default);
    Task<ServiceResult<SupportTicketMessageDto>> ReplyAsync(Guid ticketId, ReplySupportTicketRequest request, CancellationToken cancellationToken = default);
}
