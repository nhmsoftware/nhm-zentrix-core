using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
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
            IsActive = true
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
        IPasswordHashService? passwordHashService = null,
        IAccessTokenService? accessTokenService = null,
        IUnitOfWork? unitOfWork = null,
        ICurrentUserContext? currentUserContext = null)
    {
        return new AuthService(
            userRepository ?? new FakeUserRepository(),
            passwordHashService ?? new FakePasswordHashService(),
            accessTokenService ?? new FakeAccessTokenService(),
            unitOfWork ?? new FakeUnitOfWork(),
            currentUserContext ?? new FakeCurrentUserContext());
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
    }

    private sealed class FakePasswordHashService : IPasswordHashService
    {
        public string HashPassword(User user, string password) => $"hashed:{password}";

        public bool VerifyPassword(User user, string password, string passwordHash) =>
            string.Equals(passwordHash, $"hashed:{password}", StringComparison.Ordinal);
    }

    private sealed class FakeAccessTokenService : IAccessTokenService
    {
        public AccessTokenDto CreateToken(User user) =>
            new("fake-token", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc));
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
        public string? UserName { get; init; }
        public bool IsAuthenticated { get; init; }
    }
}
