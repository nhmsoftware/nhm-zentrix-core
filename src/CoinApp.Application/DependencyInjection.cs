using CoinApp.Application.Services.Market;
using CoinApp.Application.Validators.Market;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CoinApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMarketService, MarketService>();
        services.AddValidatorsFromAssemblyContaining<GetCoinBySymbolRequestValidator>();

        return services;
    }
}

