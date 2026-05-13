using CoinApp.Api.Localization;
using CoinApp.Application.Dtos.Accounts;
using CoinApp.Application.Services.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Authorize]
[Route("api")]
public sealed class AccountsController : ApiControllerBase
{
    private readonly ITradingAccountService _tradingAccountService;

    public AccountsController(ITradingAccountService tradingAccountService, ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _tradingAccountService = tradingAccountService;
    }

    [HttpGet("list-account")]
    public async Task<IActionResult> GetAccounts([FromQuery] string? type, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _tradingAccountService.GetCurrentUserAccountsAsync(type, page, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("active-protect-account")]
    public async Task<IActionResult> ToggleProtectAccount([FromBody] ToggleProtectAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _tradingAccountService.ToggleProtectAccountAsync(request, cancellationToken);
        return FromResult(result);
    }
}
