using CoinApp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace CoinApp.Api.Auth;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);

        services.AddOptions<JwtOptions>()
            .Bind(jwtSection);

        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IAccessTokenService, PersonalAccessTokenService>();

        services.AddAuthentication(PersonalAccessTokenAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, PersonalAccessTokenAuthenticationHandler>(
                PersonalAccessTokenAuthenticationHandler.SchemeName,
                _ => { });

        return services;
    }
}
