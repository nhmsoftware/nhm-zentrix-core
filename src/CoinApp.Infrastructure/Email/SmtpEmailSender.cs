using System.Net;
using System.Net.Mail;
using CoinApp.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoinApp.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailVerificationCodeAsync(
        string email,
        string fullName,
        string code,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation(
                "Email SMTP is not configured. Verification code for {Email}: {Code}, expires at {ExpiresAtUtc}.",
                email,
                code,
                expiresAtUtc);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(
                _configuration["Email:Smtp:FromEmail"] ?? "no-reply@zentrix.local",
                _configuration["Email:Smtp:FromName"] ?? "Zentrix"),
            Subject = "Zentrix email verification code",
            Body = $"""
                   Hello {fullName},

                   Your Zentrix email verification code is: {code}

                   This code expires at {expiresAtUtc:O}.
                   """,
            IsBodyHtml = false
        };

        message.To.Add(email);

        using var client = new SmtpClient(host, GetInt("Email:Smtp:Port", 587))
        {
            EnableSsl = GetBool("Email:Smtp:EnableSsl", true)
        };

        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }

    public async Task SendPasswordResetCodeAsync(
        string email,
        string fullName,
        string code,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:Smtp:Host"];

        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation(
                "Email SMTP is not configured. Password reset code for {Email}: {Code}, expires at {ExpiresAtUtc}.",
                email,
                code,
                expiresAtUtc);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(
                _configuration["Email:Smtp:FromEmail"] ?? "no-reply@zentrix.local",
                _configuration["Email:Smtp:FromName"] ?? "Zentrix"),
            Subject = "Zentrix password reset code",
            Body = $"""
                   Hello {fullName},

                   Your Zentrix password reset code is: {code}

                   This code expires at {expiresAtUtc:O}.
                   """,
            IsBodyHtml = false
        };

        message.To.Add(email);

        using var client = new SmtpClient(host, GetInt("Email:Smtp:Port", 587))
        {
            EnableSsl = GetBool("Email:Smtp:EnableSsl", true)
        };

        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }

    private int GetInt(string key, int defaultValue) =>
        int.TryParse(_configuration[key], out var value) ? value : defaultValue;

    private bool GetBool(string key, bool defaultValue) =>
        bool.TryParse(_configuration[key], out var value) ? value : defaultValue;
}
