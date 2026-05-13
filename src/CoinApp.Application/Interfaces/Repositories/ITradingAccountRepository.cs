using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;

namespace CoinApp.Application.Interfaces.Repositories;

public interface ITradingAccountRepository : IRepository<TradingAccount>
{
    IQueryable<TradingAccount> QueryByUser(Guid userId, TradingAccountType? type);
    Task<TradingAccount?> GetByIdForUserAsync(Guid accountId, Guid userId, CancellationToken cancellationToken = default);
}
