using CoinApp.Api.Localization;
using CoinApp.Application.Dtos.Market;
using CoinApp.Application.Services.Market;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Route("api/coins")]
public sealed class CoinsController : ApiControllerBase
{
    private readonly IMarketService _marketService;

    public CoinsController(IMarketService marketService, ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _marketService = marketService;
    }

    [HttpGet("{symbol}")]
    public async Task<IActionResult> GetBySymbol([FromRoute] GetCoinBySymbolRequest request, CancellationToken cancellationToken)
    {
        var result = await _marketService.GetCoinBySymbolAsync(request, cancellationToken);

        return FromResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var result = await _marketService.GetActiveCoinsAsync(cancellationToken);
        return FromResult(result);
    }
}
