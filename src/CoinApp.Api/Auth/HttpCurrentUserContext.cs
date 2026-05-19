using System.Security.Claims;
using CoinApp.Application.Common.Interfaces;

namespace CoinApp.Api.Auth;

public sealed class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    public Guid? AccessTokenId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(PersonalAccessTokenAuthenticationHandler.AccessTokenIdClaimType);
            return Guid.TryParse(value, out var accessTokenId) ? accessTokenId : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
}
