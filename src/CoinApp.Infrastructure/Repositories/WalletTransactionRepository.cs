using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public sealed class WalletTransactionRepository : BaseRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public IQueryable<WalletTransaction> QueryByUser(Guid userId)
    {
        return DbContext.WalletTransactions.AsNoTracking().Where(x => x.UserId == userId);
    }
}
