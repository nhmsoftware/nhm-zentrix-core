using System.Globalization;
using CoinApp.Api.Contracts.Responses;
using CoinApp.Application.Common.Constants;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CoinApp.Api.Localization;

public static class LocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddApiLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        services.AddScoped<ILocalizedMessageService, LocalizedMessageService>();

        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = LocalizationConstants
                .SupportedCultures
                .Select(culture => new CultureInfo(culture))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture(LocalizationConstants.DefaultCulture);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            ];
        });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var localizedMessageService = context.HttpContext.RequestServices.GetRequiredService<ILocalizedMessageService>();
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => localizedMessageService.Get(error.ErrorMessage))
                            .ToArray());

                return new BadRequestObjectResult(new ValidationErrorResponse(
                    localizedMessageService.Get(ServiceErrorCodes.ValidationFailed),
                    errors));
            };
        });

        return services;
    }

    public static IApplicationBuilder UseApiLocalization(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>();
        app.UseRequestLocalization(options.Value);

        return app;
    }
}
