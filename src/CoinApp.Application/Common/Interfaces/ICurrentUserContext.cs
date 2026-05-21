namespace CoinApp.Application.Common.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
    Guid? AccessTokenId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
