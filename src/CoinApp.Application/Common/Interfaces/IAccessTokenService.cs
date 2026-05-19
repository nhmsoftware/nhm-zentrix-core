using CoinApp.Application.Dtos.Auth;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Common.Interfaces;

public interface IAccessTokenService
{
    Task<AccessTokenDto> CreateTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(Guid accessTokenId, CancellationToken cancellationToken = default);
    Task<int> RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);
}
