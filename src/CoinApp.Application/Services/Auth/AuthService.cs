using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IAccessTokenService accessTokenService,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _accessTokenService = accessTokenService;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                ServiceErrorCodes.AuthEmailAlreadyExists,
                ServiceErrorCodes.AuthEmailAlreadyExists);
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            IsActive = true,
            ReferralCode = CreateReferralCode()
        };

        user.PasswordHash = _passwordHashService.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponseDto>.Success(CreateAuthResponse(user));
    }

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                ServiceErrorCodes.AuthInvalidCredentials,
                ServiceErrorCodes.AuthInvalidCredentials);
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                ServiceErrorCodes.UserInactive,
                ServiceErrorCodes.UserInactive);
        }

        var isPasswordValid = _passwordHashService.VerifyPassword(user, request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                ServiceErrorCodes.AuthInvalidCredentials,
                ServiceErrorCodes.AuthInvalidCredentials);
        }

        return ServiceResult<AuthResponseDto>.Success(CreateAuthResponse(user));
    }

    public async Task<ServiceResult<AuthUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<AuthUserDto>.Failure(
                ServiceErrorCodes.UserNotFound,
                ServiceErrorCodes.UserNotFound);
        }

        var user = await _userRepository.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (user is null)
        {
            return ServiceResult<AuthUserDto>.Failure(
                ServiceErrorCodes.UserNotFound,
                ServiceErrorCodes.UserNotFound);
        }

        if (!user.IsActive)
        {
            return ServiceResult<AuthUserDto>.Failure(
                ServiceErrorCodes.UserInactive,
                ServiceErrorCodes.UserInactive);
        }

        return ServiceResult<AuthUserDto>.Success(MapUser(user));
    }

    public Task<ServiceResult<LogoutResponseDto>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ServiceResult<LogoutResponseDto>.Success(new LogoutResponseDto(true)));
    }

    private AuthResponseDto CreateAuthResponse(User user)
    {
        var token = _accessTokenService.CreateToken(user);

        return new AuthResponseDto(
            MapUser(user),
            token.AccessToken,
            token.ExpiresAtUtc);
    }

    private static AuthUserDto MapUser(User user) =>
        new(user.Id, user.FullName, user.Email, user.IsActive);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string CreateReferralCode() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
