using CoinApp.Api.Localization;
using CoinApp.Application.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Authorize]
[Route("api/common")]
public sealed class CommonController : ApiControllerBase
{
    private readonly ICommonLookupService _commonLookupService;

    public CommonController(ICommonLookupService commonLookupService, ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _commonLookupService = commonLookupService;
    }

    [HttpGet("banks")]
    public async Task<IActionResult> GetBanks(CancellationToken cancellationToken)
    {
        var result = await _commonLookupService.GetBanksAsync(cancellationToken);
        return FromResult(result);
    }
}
