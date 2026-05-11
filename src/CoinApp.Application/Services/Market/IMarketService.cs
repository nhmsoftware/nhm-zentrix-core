using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Market;

namespace CoinApp.Application.Services.Market;

public interface IMarketService
{
    Task<ServiceResult<CoinDto>> GetCoinBySymbolAsync(GetCoinBySymbolRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<IReadOnlyList<CoinDto>>> GetActiveCoinsAsync(CancellationToken cancellationToken = default);
}

