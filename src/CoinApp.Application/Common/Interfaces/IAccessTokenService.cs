using CoinApp.Application.Dtos.Auth;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Common.Interfaces;

public interface IAccessTokenService
{
    AccessTokenDto CreateToken(User user);
}

