using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;

namespace CoinApp.Infrastructure.Repositories;

public sealed class SupportTicketMessageRepository : BaseRepository<SupportTicketMessage>, ISupportTicketMessageRepository
{
    public SupportTicketMessageRepository(AppDbContext dbContext) : base(dbContext)
    {
    }
}
