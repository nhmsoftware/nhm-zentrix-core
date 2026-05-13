using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class TradingAccountRepository : BaseRepository<TradingAccount>, ITradingAccountRepository
{
    public TradingAccountRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public IQueryable<TradingAccount> QueryByUser(Guid userId, TradingAccountType? type)
    {
        var query = DbContext.TradingAccounts.AsNoTracking().Where(x => x.UserId == userId);

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        return query;
    }

    public Task<TradingAccount?> GetByIdForUserAsync(Guid accountId, Guid userId, CancellationToken cancellationToken = default)
    {
        return DbContext.TradingAccounts.FirstOrDefaultAsync(x => x.Id == accountId && x.UserId == userId, cancellationToken);
    }
}
