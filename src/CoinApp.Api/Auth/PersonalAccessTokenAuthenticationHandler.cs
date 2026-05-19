using System.Security.Claims;
using System.Text.Encodings.Web;
using CoinApp.Api.Contracts.Responses;
using CoinApp.Api.Localization;
using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CoinApp.Api.Auth;

public sealed class PersonalAccessTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Bearer";
    public const string AccessTokenIdClaimType = "access_token_id";

    private readonly IPersonalAccessTokenRepository _personalAccessTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizedMessageService _localizedMessageService;

    public PersonalAccessTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IPersonalAccessTokenRepository personalAccessTokenRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILocalizedMessageService localizedMessageService)
        : base(options, logger, encoder)
    {
        _personalAccessTokenRepository = personalAccessTokenRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _localizedMessageService = localizedMessageService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorization))
        {
            return AuthenticateResult.NoResult();
        }

        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.Fail("Invalid authorization scheme.");
        }

        var plainToken = authorization["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(plainToken))
        {
            return AuthenticateResult.Fail("Missing bearer token.");
        }

        var tokenHash = PersonalAccessTokenHasher.HashToken(plainToken);
        var accessToken = await _personalAccessTokenRepository.GetByTokenHashAsync(tokenHash, Context.RequestAborted);

        if (accessToken is null ||
            accessToken.RevokedAtUtc is not null ||
            accessToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return AuthenticateResult.Fail("Invalid bearer token.");
        }

        var user = await _userRepository.GetByIdAsync(accessToken.UserId, Context.RequestAborted);

        if (user is null || !user.IsActive)
        {
            return AuthenticateResult.Fail("Invalid bearer token.");
        }

        accessToken.LastUsedAtUtc = DateTime.UtcNow;
        _personalAccessTokenRepository.Update(accessToken);
        await _unitOfWork.SaveChangesAsync(Context.RequestAborted);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(AccessTokenIdClaimType, accessToken.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (Response.HasStarted)
        {
            return;
        }

        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json; charset=utf-8";

        await Response.WriteAsJsonAsync(new ApiErrorResponse(
            _localizedMessageService.Get(ServiceErrorCodes.AuthUnauthenticated)));
    }
}
