using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;

namespace CoinApp.Application.Interfaces.Repositories;

public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    IQueryable<SupportTicket> QueryByUser(Guid userId, string? keyword, SupportTicketStatus? status);
    Task<SupportTicket?> GetThreadForUserAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default);
}
