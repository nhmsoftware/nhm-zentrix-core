using CoinApp.Application.Services.Auth;
using CoinApp.Application.Services.Accounts;
using CoinApp.Application.Services.Common;
using CoinApp.Application.Services.Config;
using CoinApp.Application.Services.Market;
using CoinApp.Application.Services.Profile;
using CoinApp.Application.Services.SupportTickets;
using CoinApp.Application.Services.Wallet;
using CoinApp.Application.Common.Options;
using CoinApp.Application.Validators.Auth;
using CoinApp.Application.Validators.Market;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoinApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMarketService, MarketService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<ICommonLookupService, CommonLookupService>();
        services.AddScoped<IAppConfigService, AppConfigService>();
        services.AddScoped<ITradingAccountService, TradingAccountService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<ISupportTicketService, SupportTicketService>();
        services.AddSingleton(new EmailVerificationOptions());
        services.AddSingleton(new PasswordResetOptions());
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<GetCoinBySymbolRequestValidator>();

        return services;
    }
}
