using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class PasswordResetCodeRepository : BaseRepository<PasswordResetCode>, IPasswordResetCodeRepository
{
    public PasswordResetCodeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<PasswordResetCode?> GetLatestPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return DbContext.PasswordResetCodes
            .Where(x => x.Email == email && x.ConsumedAtUtc == null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PasswordResetCode>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbContext.PasswordResetCodes
            .Where(x => x.Email == email && x.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);
    }

    public Task<PasswordResetCode?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default)
    {
        return DbContext.PasswordResetCodes
            .FirstOrDefaultAsync(x => x.ResetTokenHash == resetTokenHash && x.ConsumedAtUtc == null, cancellationToken);
    }
}
