using CoinApp.Api.Contracts.Responses;
using CoinApp.Api.Localization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Route("api/localization")]
public sealed class LocalizationController : ControllerBase
{
    private readonly ILocalizedMessageService _localizedMessageService;

    public LocalizationController(ILocalizedMessageService localizedMessageService)
    {
        _localizedMessageService = localizedMessageService;
    }

    [HttpPost("culture")]
    public IActionResult SetCulture([FromQuery] string culture)
    {
        if (!LocalizationConstants.SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase))
        {
            var messageKey = "culture.unsupported";
            return BadRequest(new ApiErrorResponse(
                messageKey,
                messageKey,
                _localizedMessageService.Get(messageKey)));
        }

        var normalizedCulture = culture.Trim().ToLowerInvariant();
        var requestCulture = new RequestCulture(normalizedCulture);

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(requestCulture),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                HttpOnly = false,
                SameSite = SameSiteMode.Lax
            });

        return NoContent();
    }
}
