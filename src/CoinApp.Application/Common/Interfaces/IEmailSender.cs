namespace CoinApp.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendEmailVerificationCodeAsync(
        string email,
        string fullName,
        string code,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default);
}
