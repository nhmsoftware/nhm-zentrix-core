using CoinApp.Api.Localization;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Route("api/admin/auth")]
public sealed class AdminAuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public AdminAuthController(
        IAuthService authService,
        ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.AdminLoginAsync(request, cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentAdmin(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentAdminUserAsync(cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(cancellationToken);
        return FromResult(result);
    }
}
