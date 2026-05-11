namespace CoinApp.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}

