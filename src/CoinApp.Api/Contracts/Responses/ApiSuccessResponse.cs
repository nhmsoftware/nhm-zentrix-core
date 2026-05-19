namespace CoinApp.Api.Contracts.Responses;

public sealed record ApiSuccessResponse<T>(
    string Message,
    T? Data);
