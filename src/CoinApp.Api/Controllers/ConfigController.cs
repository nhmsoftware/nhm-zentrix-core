using CoinApp.Api.Localization;
using CoinApp.Application.Services.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Authorize]
[Route("api/config")]
public sealed class ConfigController : ApiControllerBase
{
    private readonly IAppConfigService _appConfigService;

    public ConfigController(IAppConfigService appConfigService, ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _appConfigService = appConfigService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetPublicConfigs(CancellationToken cancellationToken)
    {
        var result = await _appConfigService.GetPublicConfigsAsync(cancellationToken);
        return FromResult(result);
    }
}
