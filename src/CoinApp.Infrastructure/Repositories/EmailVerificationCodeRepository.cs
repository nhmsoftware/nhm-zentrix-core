using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class EmailVerificationCodeRepository : BaseRepository<EmailVerificationCode>, IEmailVerificationCodeRepository
{
    public EmailVerificationCodeRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<EmailVerificationCode?> GetLatestPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return DbContext.EmailVerificationCodes
            .Where(x => x.Email == email && x.ConsumedAtUtc == null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailVerificationCode>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbContext.EmailVerificationCodes
            .Where(x => x.Email == email && x.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);
    }
}
