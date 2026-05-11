namespace CoinApp.Api.Contracts.Responses;

public sealed record ApiErrorResponse(
    string ErrorCode,
    string MessageKey,
    string Message);

