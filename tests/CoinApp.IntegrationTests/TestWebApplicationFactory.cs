using CoinApp.Infrastructure.Persistence;
using CoinApp.Application.Common.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CoinApp.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<global::Program>
{
    private readonly string _databaseName = $"coinapp-tests-{Guid.NewGuid():N}";

    public TestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            "Host=localhost;Database=coinapp_tests;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "CoinApp.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "CoinApp.Tests.Client");
        Environment.SetEnvironmentVariable(
            "Jwt__SigningKey",
            "IntegrationTestsSigningKey_123456789012345678901234567890");
        Environment.SetEnvironmentVariable("Jwt__ExpirationMinutes", "60");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=coinapp_tests;Username=test;Password=test",
                ["Jwt:Issuer"] = "CoinApp.Tests",
                ["Jwt:Audience"] = "CoinApp.Tests.Client",
                ["Jwt:SigningKey"] = "IntegrationTestsSigningKey_123456789012345678901234567890",
                ["Jwt:ExpirationMinutes"] = "60",
                ["EmailVerification:ExposeCodeInResponse"] = "true",
                ["EmailVerification:CodeExpirationMinutes"] = "15",
                ["PasswordReset:ExposeCodeInResponse"] = "true",
                ["PasswordReset:CodeExpirationMinutes"] = "15",
                ["PasswordReset:ResetTokenExpirationMinutes"] = "15",
                ["PasswordReset:MaxVerifyAttempts"] = "5"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<EmailVerificationOptions>();
            services.RemoveAll(typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AppDbContext>));

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddSingleton(new EmailVerificationOptions
            {
                CodeExpirationMinutes = 15,
                ExposeCodeInResponse = true
            });
            services.AddSingleton(new PasswordResetOptions
            {
                CodeExpirationMinutes = 15,
                ResetTokenExpirationMinutes = 15,
                MaxVerifyAttempts = 5,
                ExposeCodeInResponse = true
            });
        });
    }
}
