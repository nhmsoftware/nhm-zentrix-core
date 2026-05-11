namespace CoinApp.Application.Common.Results;

public sealed class ServiceResult<T>
{
    private ServiceResult(bool succeeded, T? data, string? errorCode, string? messageKey)
    {
        Succeeded = succeeded;
        Data = data;
        ErrorCode = errorCode;
        MessageKey = messageKey;
    }

    public bool Succeeded { get; }
    public T? Data { get; }
    public string? ErrorCode { get; }
    public string? MessageKey { get; }

    public static ServiceResult<T> Success(T data, string? messageKey = null) =>
        new(true, data, null, messageKey);

    public static ServiceResult<T> Failure(string errorCode, string? messageKey = null) =>
        new(false, default, errorCode, messageKey);
}

