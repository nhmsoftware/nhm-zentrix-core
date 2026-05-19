using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Interfaces.Repositories;

public interface IEmailVerificationCodeRepository : IRepository<EmailVerificationCode>
{
    Task<EmailVerificationCode?> GetLatestPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailVerificationCode>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
}
