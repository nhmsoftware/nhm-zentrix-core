using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class AppConfigRepository : BaseRepository<AppConfig>, IAppConfigRepository
{
    public AppConfigRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<AppConfig>> GetPublicAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.AppConfigs
            .AsNoTracking()
            .Where(x => x.IsPublic)
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }

    public Task<AppConfig?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return DbContext.AppConfigs.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }
}
