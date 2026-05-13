using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class SupportTicketRepository : BaseRepository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public IQueryable<SupportTicket> QueryByUser(Guid userId, string? keyword, SupportTicketStatus? status)
    {
        var query = DbContext.SupportTickets.AsNoTracking().Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();
            query = query.Where(x => x.Message.Contains(normalizedKeyword));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query;
    }

    public Task<SupportTicket?> GetThreadForUserAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default)
    {
        return DbContext.SupportTickets
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == ticketId && x.UserId == userId, cancellationToken);
    }
}
