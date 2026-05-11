using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Market;
using CoinApp.Application.Interfaces.Repositories;

namespace CoinApp.Application.Services.Market;

public sealed class MarketService : IMarketService
{
    private readonly ICoinRepository _coinRepository;

    public MarketService(ICoinRepository coinRepository)
    {
        _coinRepository = coinRepository;
    }

    public async Task<ServiceResult<CoinDto>> GetCoinBySymbolAsync(GetCoinBySymbolRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var symbol = request.Symbol.Trim().ToUpperInvariant();
        var coin = await _coinRepository.GetBySymbolAsync(symbol, cancellationToken);

        if (coin is null)
        {
            return ServiceResult<CoinDto>.Failure(ServiceErrorCodes.CoinNotFound, ServiceErrorCodes.CoinNotFound);
        }

        return ServiceResult<CoinDto>.Success(new CoinDto(
            coin.Id,
            coin.Symbol,
            coin.Name,
            coin.Decimals,
            coin.IsActive));
    }

    public async Task<ServiceResult<IReadOnlyList<CoinDto>>> GetActiveCoinsAsync(CancellationToken cancellationToken = default)
    {
        var coins = await _coinRepository.GetActiveAsync(cancellationToken);
        var result = coins
            .OrderBy(x => x.Symbol)
            .Select(x => new CoinDto(x.Id, x.Symbol, x.Name, x.Decimals, x.IsActive))
            .ToList();

        return ServiceResult<IReadOnlyList<CoinDto>>.Success(result);
    }
}
