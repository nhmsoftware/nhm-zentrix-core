using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface IWalletTransactionRepository : IRepository<WalletTransaction>
{
    IQueryable<WalletTransaction> QueryByUser(Guid userId);
}
