using Microsoft.Extensions.Localization;

namespace CoinApp.Api.Localization;

public sealed class LocalizedMessageService : ILocalizedMessageService
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LocalizedMessageService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        var localized = _localizer[key];
        return localized.ResourceNotFound ? key : localized.Value;
    }
}

