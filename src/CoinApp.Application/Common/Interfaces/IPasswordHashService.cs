using CoinApp.Domain.Entities;

namespace CoinApp.Application.Common.Interfaces;

public interface IPasswordHashService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}

