using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Config;
using CoinApp.Application.Interfaces.Repositories;

namespace CoinApp.Application.Services.Config;

public sealed class AppConfigService : IAppConfigService
{
    private readonly IAppConfigRepository _appConfigRepository;

    public AppConfigService(IAppConfigRepository appConfigRepository)
    {
        _appConfigRepository = appConfigRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<AppConfigDto>>> GetPublicConfigsAsync(CancellationToken cancellationToken = default)
    {
        var configs = await _appConfigRepository.GetPublicAsync(cancellationToken);
        var result = configs
            .OrderBy(x => x.Key)
            .Select(x => new AppConfigDto(x.Key, x.Value, x.Description))
            .ToList();

        return ServiceResult<IReadOnlyList<AppConfigDto>>.Success(result);
    }
}
