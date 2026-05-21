using CoinApp.Application.Common.Interfaces;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CoinApp.Infrastructure.Persistence;

public static class AdminUserSeeder
{
    public static async Task SeedAdminUserAsync(this IServiceProvider services, IConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var enabled = bool.TryParse(configuration["AdminSeed:Enabled"], out var parsedEnabled) && parsedEnabled;

        if (!enabled)
        {
            return;
        }

        var email = configuration["AdminSeed:Email"]?.Trim().ToLowerInvariant();
        var password = configuration["AdminSeed:Password"];
        var fullName = configuration["AdminSeed:FullName"]?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        var existingUser = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.AdminRole == AdminRole.None)
            {
                existingUser.AdminRole = AdminRole.SuperAdmin;
                existingUser.IsActive = true;
                existingUser.EmailVerifiedAtUtc ??= DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return;
        }

        var admin = new User
        {
            FullName = string.IsNullOrWhiteSpace(fullName) ? "Super Admin" : fullName,
            Email = email,
            AdminRole = AdminRole.SuperAdmin,
            IsActive = true,
            EmailVerifiedAtUtc = DateTime.UtcNow,
            ReferralCode = "ADMIN001"
        };

        admin.PasswordHash = passwordHashService.HashPassword(admin, password);

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
