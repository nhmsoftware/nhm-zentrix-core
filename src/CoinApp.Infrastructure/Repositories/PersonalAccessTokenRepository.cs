using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class PersonalAccessTokenRepository : BaseRepository<PersonalAccessToken>, IPersonalAccessTokenRepository
{
    public PersonalAccessTokenRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<PersonalAccessToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return DbContext.PersonalAccessTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<IReadOnlyList<PersonalAccessToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbContext.PersonalAccessTokens
            .Where(x => x.UserId == userId && x.RevokedAtUtc == null && x.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }
}
