using CoinApp.Api.Localization;
using CoinApp.Application.Common.Constants;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly ILocalizedMessageService _localizedMessageService;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        ILocalizedMessageService localizedMessageService)
    {
        _next = next;
        _logger = logger;
        _localizedMessageService = localizedMessageService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception.");

            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = _localizedMessageService.Get("common.error_title"),
                Detail = _localizedMessageService.Get(ServiceErrorCodes.UnexpectedError)
            });
        }
    }
}
