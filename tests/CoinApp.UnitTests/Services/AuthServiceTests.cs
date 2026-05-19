using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Options;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Application.Services.Auth;
using CoinApp.Domain.Entities;
using Xunit;

namespace CoinApp.UnitTests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_ReturnsFailure_WhenEmailAlreadyExists()
    {
        var userRepository = new FakeUserRepository(new User
        {
            FullName = "Existing User",
            Email = "alice@example.com",
            PasswordHash = "hashed:Password123!"
        });

        var service = CreateService(userRepository);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Alice",
            Email = "ALICE@example.com",
            Password = "Password123!"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorCodes.AuthEmailAlreadyExists, result.ErrorCode);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        var userRepository = new FakeUserRepository();
        var unitOfWork = new FakeUnitOfWork();
        var service = CreateService(userRepository, unitOfWork: unitOfWork);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            FullName = "Alice Example",
            Email = "ALICE@example.com",
            Password = "Password123!"
        });

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("alice@example.com", result.Data!.User.Email);
        Assert.Equal("Alice Example", result.Data.User.FullName);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task LoginAsync_ReturnsFailure_WhenPasswordIsInvalid()
    {
        var user = new User
        {
            FullName = "Alice Example",
            Email = "alice@example.com",
            PasswordHash = "hashed:CorrectPassword123!",
            IsActive = true,
            EmailVerifiedAtUtc = DateTime.UtcNow
        };

        var service = CreateService(new FakeUserRepository(user));

        var result = await service.LoginAsync(new LoginRequest
        {
            Email = "alice@example.com",
            Password = "WrongPassword123!"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorCodes.AuthInvalidCredentials, result.ErrorCode);
    }

    private static AuthService CreateService(
        IUserRepository? userRepository = null,
        IEmailVerificationCodeRepository? emailVerificationCodeRepository = null,
        IPasswordResetCodeRepository? passwordResetCodeRepository = null,
        IPasswordHashService? passwordHashService = null,
        IAccessTokenService? accessTokenService = null,
        IEmailSender? emailSender = null,
        IUnitOfWork? unitOfWork = null,
        ICurrentUserContext? currentUserContext = null,
        EmailVerificationOptions? emailVerificationOptions = null,
        PasswordResetOptions? passwordResetOptions = null)
    {
        return new AuthService(
            userRepository ?? new FakeUserRepository(),
            emailVerificationCodeRepository ?? new FakeEmailVerificationCodeRepository(),
            passwordResetCodeRepository ?? new FakePasswordResetCodeRepository(),
            passwordHashService ?? new FakePasswordHashService(),
            accessTokenService ?? new FakeAccessTokenService(),
            emailSender ?? new FakeEmailSender(),
            unitOfWork ?? new FakeUnitOfWork(),
            currentUserContext ?? new FakeCurrentUserContext(),
            emailVerificationOptions ?? new EmailVerificationOptions(),
            passwordResetOptions ?? new PasswordResetOptions());
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<User> _users;

        public FakeUserRepository(params User[] users)
        {
            _users = users.ToList();
        }

        public IQueryable<User> Query() => _users.AsQueryable();

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(User entity, CancellationToken cancellationToken = default)
        {
            _users.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(User entity)
        {
        }

        public void Delete(User entity)
        {
            _users.Remove(entity);
        }

        public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.AsQueryable().Any(predicate));

        public Task<CoinApp.Application.Common.Results.PaginatedResult<User>> PaginateAsync(IQueryable<User> query, int page, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(new CoinApp.Application.Common.Results.PaginatedResult<User>(_users, 1, _users.Count, _users.Count));

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.FirstOrDefault(x => x.Email == email));

        public Task<User?> GetByReferralCodeAsync(string referralCode, CancellationToken cancellationToken = default) =>
            Task.FromResult(_users.FirstOrDefault(x => x.ReferralCode == referralCode));
    }

    private sealed class FakeEmailVerificationCodeRepository : IEmailVerificationCodeRepository
    {
        private readonly List<EmailVerificationCode> _codes = new();

        public IQueryable<EmailVerificationCode> Query() => _codes.AsQueryable();

        public Task<EmailVerificationCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(EmailVerificationCode entity, CancellationToken cancellationToken = default)
        {
            _codes.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(EmailVerificationCode entity)
        {
        }

        public void Delete(EmailVerificationCode entity)
        {
            _codes.Remove(entity);
        }

        public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<EmailVerificationCode, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes.AsQueryable().Any(predicate));

        public Task<CoinApp.Application.Common.Results.PaginatedResult<EmailVerificationCode>> PaginateAsync(IQueryable<EmailVerificationCode> query, int page, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(new CoinApp.Application.Common.Results.PaginatedResult<EmailVerificationCode>(_codes, 1, _codes.Count, _codes.Count));

        public Task<EmailVerificationCode?> GetLatestPendingByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes
                .Where(x => x.Email == email && x.ConsumedAtUtc == null)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefault());

        public Task<IReadOnlyList<EmailVerificationCode>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<EmailVerificationCode>>(_codes
                .Where(x => x.Email == email && x.ConsumedAtUtc == null)
                .ToList());
    }

    private sealed class FakePasswordResetCodeRepository : IPasswordResetCodeRepository
    {
        private readonly List<PasswordResetCode> _codes = new();

        public IQueryable<PasswordResetCode> Query() => _codes.AsQueryable();

        public Task<PasswordResetCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes.FirstOrDefault(x => x.Id == id));

        public Task AddAsync(PasswordResetCode entity, CancellationToken cancellationToken = default)
        {
            _codes.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(PasswordResetCode entity)
        {
        }

        public void Delete(PasswordResetCode entity)
        {
            _codes.Remove(entity);
        }

        public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<PasswordResetCode, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes.AsQueryable().Any(predicate));

        public Task<CoinApp.Application.Common.Results.PaginatedResult<PasswordResetCode>> PaginateAsync(IQueryable<PasswordResetCode> query, int page, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(new CoinApp.Application.Common.Results.PaginatedResult<PasswordResetCode>(_codes, 1, _codes.Count, _codes.Count));

        public Task<PasswordResetCode?> GetLatestPendingByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes
                .Where(x => x.Email == email && x.ConsumedAtUtc == null)
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefault());

        public Task<IReadOnlyList<PasswordResetCode>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PasswordResetCode>>(_codes
                .Where(x => x.Email == email && x.ConsumedAtUtc == null)
                .ToList());

        public Task<PasswordResetCode?> GetByResetTokenHashAsync(string resetTokenHash, CancellationToken cancellationToken = default) =>
            Task.FromResult(_codes.FirstOrDefault(x => x.ResetTokenHash == resetTokenHash && x.ConsumedAtUtc == null));
    }

    private sealed class FakePasswordHashService : IPasswordHashService
    {
        public string HashPassword(User user, string password) => $"hashed:{password}";

        public bool VerifyPassword(User user, string password, string passwordHash) =>
            string.Equals(passwordHash, $"hashed:{password}", StringComparison.Ordinal);
    }

    private sealed class FakeAccessTokenService : IAccessTokenService
    {
        public Task<AccessTokenDto> CreateTokenAsync(User user, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AccessTokenDto("fake-token", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)));

        public Task<bool> RevokeTokenAsync(Guid accessTokenId, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<int> RevokeUserTokensAsync(Guid userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        public Task SendEmailVerificationCodeAsync(
            string email,
            string fullName,
            string code,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SendPasswordResetCodeAsync(
            string email,
            string fullName,
            string code,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            await action(cancellationToken);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
        {
            var result = await action(cancellationToken);
            await SaveChangesAsync(cancellationToken);
            return result;
        }
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public Guid? UserId { get; init; }
        public Guid? AccessTokenId { get; init; }
        public string? UserName { get; init; }
        public bool IsAuthenticated { get; init; }
    }
}
