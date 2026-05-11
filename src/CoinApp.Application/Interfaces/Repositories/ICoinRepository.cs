using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface ICoinRepository : IRepository<Coin>
{
    Task<Coin?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Coin>> GetActiveAsync(CancellationToken cancellationToken = default);
}
