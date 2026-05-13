using CoinApp.Api.Localization;
using CoinApp.Application.Dtos.Wallet;
using CoinApp.Application.Services.Wallet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Authorize]
[Route("api/wallet")]
public sealed class WalletController : ApiControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService, ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _walletService = walletService;
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        var result = await _walletService.GetCurrentUserTransactionsAsync(page, cancellationToken);
        return FromResult(result);
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawWalletRequest request, CancellationToken cancellationToken)
    {
        var result = await _walletService.WithdrawAsync(request, cancellationToken);
        return FromResult(result);
    }
}
