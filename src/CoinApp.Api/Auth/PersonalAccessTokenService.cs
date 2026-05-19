using System.Security.Cryptography;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using Microsoft.Extensions.Options;

namespace CoinApp.Api.Auth;

public sealed class PersonalAccessTokenService : IAccessTokenService
{
    private const string DefaultTokenName = "api-token";

    private readonly IPersonalAccessTokenRepository _personalAccessTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtOptions _options;

    public PersonalAccessTokenService(
        IPersonalAccessTokenRepository personalAccessTokenRepository,
        IUnitOfWork unitOfWork,
        IOptions<JwtOptions> options)
    {
        _personalAccessTokenRepository = personalAccessTokenRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<AccessTokenDto> CreateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        var plainToken = CreatePlainToken();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes > 0
            ? _options.ExpirationMinutes
            : 60);

        await _personalAccessTokenRepository.AddAsync(new PersonalAccessToken
        {
            UserId = user.Id,
            Name = DefaultTokenName,
            TokenHash = PersonalAccessTokenHasher.HashToken(plainToken),
            ExpiresAtUtc = expiresAtUtc
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AccessTokenDto(plainToken, expiresAtUtc);
    }

    public async Task<bool> RevokeTokenAsync(Guid accessTokenId, CancellationToken cancellationToken = default)
    {
        var token = await _personalAccessTokenRepository.GetByIdAsync(accessTokenId, cancellationToken);

        if (token is null || token.RevokedAtUtc is not null)
        {
            return false;
        }

        token.RevokedAtUtc = DateTime.UtcNow;
        _personalAccessTokenRepository.Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _personalAccessTokenRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        if (tokens.Count == 0)
        {
            return 0;
        }

        var revokedAtUtc = DateTime.UtcNow;

        foreach (var token in tokens)
        {
            token.RevokedAtUtc = revokedAtUtc;
            _personalAccessTokenRepository.Update(token);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tokens.Count;
    }

    private static string CreatePlainToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
}
