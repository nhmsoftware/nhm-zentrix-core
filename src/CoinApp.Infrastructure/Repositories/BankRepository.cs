using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class BankRepository : BaseRepository<Bank>, IBankRepository
{
    public BankRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Bank>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Banks
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.ShortName)
            .ToListAsync(cancellationToken);
    }
}
