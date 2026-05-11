namespace CoinApp.Api.Contracts.Responses;

public sealed record ValidationErrorResponse(
    string ErrorCode,
    string MessageKey,
    string Message,
    IReadOnlyDictionary<string, string[]> Errors);

