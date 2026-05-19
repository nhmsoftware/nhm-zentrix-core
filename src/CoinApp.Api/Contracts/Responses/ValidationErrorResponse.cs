namespace CoinApp.Api.Contracts.Responses;

public sealed record ValidationErrorResponse(
    string Message,
    IReadOnlyDictionary<string, string[]> Errors);
