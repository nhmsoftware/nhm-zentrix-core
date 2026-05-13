using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface IAppConfigRepository : IRepository<AppConfig>
{
    Task<IReadOnlyList<AppConfig>> GetPublicAsync(CancellationToken cancellationToken = default);
    Task<AppConfig?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}
