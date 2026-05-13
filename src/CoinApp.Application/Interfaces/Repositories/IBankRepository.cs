using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface IBankRepository : IRepository<Bank>
{
    Task<IReadOnlyList<Bank>> GetActiveAsync(CancellationToken cancellationToken = default);
}
