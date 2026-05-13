using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Config;

namespace CoinApp.Application.Services.Config;

public interface IAppConfigService
{
    Task<ServiceResult<IReadOnlyList<AppConfigDto>>> GetPublicConfigsAsync(CancellationToken cancellationToken = default);
}
