using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface IPersonalAccessTokenRepository : IRepository<PersonalAccessToken>
{
    Task<PersonalAccessToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PersonalAccessToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
