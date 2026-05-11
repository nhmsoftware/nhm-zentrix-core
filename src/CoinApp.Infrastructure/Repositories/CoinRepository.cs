using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class CoinRepository : BaseRepository<Coin>, ICoinRepository
{
    public CoinRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Coin?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default)
    {
        return DbContext.Coins.AsNoTracking().FirstOrDefaultAsync(x => x.Symbol == symbol, cancellationToken);
    }

    public async Task<IReadOnlyList<Coin>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Coins
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Symbol)
            .ToListAsync(cancellationToken);
    }
}
