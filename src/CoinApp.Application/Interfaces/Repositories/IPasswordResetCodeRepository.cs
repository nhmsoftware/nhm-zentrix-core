using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface IPasswordResetCodeRepository : IRepository<PasswordResetCode>
{
    Task<PasswordResetCode?> GetLatestPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PasswordResetCode>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<PasswordResetCode?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default);
}
