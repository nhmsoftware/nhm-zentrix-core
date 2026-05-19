using System.Security.Cryptography;
using System.Text;
using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Options;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationCodeRepository _emailVerificationCodeRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly EmailVerificationOptions _emailVerificationOptions;

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationCodeRepository emailVerificationCodeRepository,
        IPasswordHashService passwordHashService,
        IAccessTokenService accessTokenService,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        EmailVerificationOptions emailVerificationOptions)
    {
        _userRepository = userRepository;
        _emailVerificationCodeRepository = emailVerificationCodeRepository;
        _passwordHashService = passwordHashService;
        _accessTokenService = accessTokenService;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _emailVerificationOptions = emailVerificationOptions;
    }

    public async Task<ServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (existingUser is not null)
        {
            return ServiceResult<RegisterResponseDto>.Failure(
                ServiceErrorCodes.AuthEmailAlreadyExists,
                ServiceErrorCodes.AuthEmailAlreadyExists);
        }

        User? referrer = null;
        var normalizedReferralCode = NormalizeOptionalCode(request.ReferralCode);

        if (!string.IsNullOrWhiteSpace(normalizedReferralCode))
        {
            referrer = await _userRepository.GetByReferralCodeAsync(normalizedReferralCode, cancellationToken);

            if (referrer is null)
            {
                return ServiceResult<RegisterResponseDto>.Failure(
                    ServiceErrorCodes.AuthReferralCodeInvalid,
                    ServiceErrorCodes.AuthReferralCodeInvalid);
            }
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            IsActive = true,
            ReferrerId = referrer?.Id,
            ReferralCode = CreateReferralCode()
        };

        user.PasswordHash = _passwordHashService.HashPassword(user, request.Password);

        var verificationCode = CreateVerificationCode();
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(GetCodeExpirationMinutes());
        var verification = new EmailVerificationCode
        {
            UserId = user.Id,
            Email = user.Email,
            CodeHash = HashVerificationCode(user.Email, verificationCode),
            ExpiresAtUtc = expiresAtUtc
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _emailVerificationCodeRepository.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailSender.SendEmailVerificationCodeAsync(user.Email, user.FullName, verificationCode, expiresAtUtc, cancellationToken);

        return ServiceResult<RegisterResponseDto>.Success(new RegisterResponseDto(
            MapUser(user),
            true,
            expiresAtUtc,
            GetExposedCode(verificationCode)));
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

        if (user.EmailVerifiedAtUtc is null)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                ServiceErrorCodes.AuthEmailNotVerified,
                ServiceErrorCodes.AuthEmailNotVerified);
        }

        var isPasswordValid = _passwordHashService.VerifyPassword(user, request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            return ServiceResult<AuthResponseDto>.Failure(
                ServiceErrorCodes.AuthInvalidCredentials,
                ServiceErrorCodes.AuthInvalidCredentials);
        }

        return ServiceResult<AuthResponseDto>.Success(await CreateAuthResponseAsync(user, cancellationToken));
    }

    public async Task<ServiceResult<EmailVerificationResponseDto>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return ServiceResult<EmailVerificationResponseDto>.Failure(
                ServiceErrorCodes.UserNotFound,
                ServiceErrorCodes.UserNotFound);
        }

        if (user.EmailVerifiedAtUtc is not null)
        {
            return ServiceResult<EmailVerificationResponseDto>.Success(CreateEmailVerificationResponse(user, null, null));
        }

        var verification = await _emailVerificationCodeRepository.GetLatestPendingByEmailAsync(normalizedEmail, cancellationToken);

        if (verification is null)
        {
            return ServiceResult<EmailVerificationResponseDto>.Failure(
                ServiceErrorCodes.AuthEmailVerificationCodeInvalid,
                ServiceErrorCodes.AuthEmailVerificationCodeInvalid);
        }

        if (verification.ExpiresAtUtc < DateTime.UtcNow)
        {
            return ServiceResult<EmailVerificationResponseDto>.Failure(
                ServiceErrorCodes.AuthEmailVerificationCodeExpired,
                ServiceErrorCodes.AuthEmailVerificationCodeExpired);
        }

        var codeHash = HashVerificationCode(normalizedEmail, request.Code);

        if (!string.Equals(verification.CodeHash, codeHash, StringComparison.Ordinal))
        {
            return ServiceResult<EmailVerificationResponseDto>.Failure(
                ServiceErrorCodes.AuthEmailVerificationCodeInvalid,
                ServiceErrorCodes.AuthEmailVerificationCodeInvalid);
        }

        var verifiedAtUtc = DateTime.UtcNow;
        verification.ConsumedAtUtc = verifiedAtUtc;
        user.EmailVerifiedAtUtc = verifiedAtUtc;

        _emailVerificationCodeRepository.Update(verification);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<EmailVerificationResponseDto>.Success(CreateEmailVerificationResponse(user, null, null));
    }

    public async Task<ServiceResult<EmailVerificationResponseDto>> ResendEmailVerificationAsync(ResendEmailVerificationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return ServiceResult<EmailVerificationResponseDto>.Failure(
                ServiceErrorCodes.UserNotFound,
                ServiceErrorCodes.UserNotFound);
        }

        if (user.EmailVerifiedAtUtc is not null)
        {
            return ServiceResult<EmailVerificationResponseDto>.Success(CreateEmailVerificationResponse(user, null, null));
        }

        var now = DateTime.UtcNow;
        var pendingCodes = await _emailVerificationCodeRepository.GetPendingByEmailAsync(normalizedEmail, cancellationToken);

        foreach (var pendingCode in pendingCodes)
        {
            pendingCode.ConsumedAtUtc = now;
            _emailVerificationCodeRepository.Update(pendingCode);
        }

        var verificationCode = CreateVerificationCode();
        var expiresAtUtc = now.AddMinutes(GetCodeExpirationMinutes());
        var verification = new EmailVerificationCode
        {
            UserId = user.Id,
            Email = user.Email,
            CodeHash = HashVerificationCode(user.Email, verificationCode),
            ExpiresAtUtc = expiresAtUtc
        };

        await _emailVerificationCodeRepository.AddAsync(verification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailSender.SendEmailVerificationCodeAsync(user.Email, user.FullName, verificationCode, expiresAtUtc, cancellationToken);

        return ServiceResult<EmailVerificationResponseDto>.Success(CreateEmailVerificationResponse(user, expiresAtUtc, GetExposedCode(verificationCode)));
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

    public async Task<ServiceResult<LogoutResponseDto>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.AccessTokenId.HasValue)
        {
            return ServiceResult<LogoutResponseDto>.Failure(
                ServiceErrorCodes.AuthUnauthenticated,
                ServiceErrorCodes.AuthUnauthenticated);
        }

        var revoked = await _accessTokenService.RevokeTokenAsync(_currentUserContext.AccessTokenId.Value, cancellationToken);

        if (!revoked)
        {
            return ServiceResult<LogoutResponseDto>.Failure(
                ServiceErrorCodes.AuthUnauthenticated,
                ServiceErrorCodes.AuthUnauthenticated);
        }

        return ServiceResult<LogoutResponseDto>.Success(new LogoutResponseDto(true));
    }

    private async Task<AuthResponseDto> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var token = await _accessTokenService.CreateTokenAsync(user, cancellationToken);

        return new AuthResponseDto(
            MapUser(user),
            token.AccessToken,
            token.ExpiresAtUtc);
    }

    private EmailVerificationResponseDto CreateEmailVerificationResponse(
        User user,
        DateTime? verificationCodeExpiresAtUtc,
        string? verificationCode) =>
        new(
            user.Email,
            user.EmailVerifiedAtUtc is not null,
            user.EmailVerifiedAtUtc,
            verificationCodeExpiresAtUtc,
            GetExposedCode(verificationCode));

    private static AuthUserDto MapUser(User user) =>
        new(user.Id, user.FullName, user.Email, user.IsActive, user.EmailVerifiedAtUtc is not null);

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string? NormalizeOptionalCode(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

    private static string CreateReferralCode() =>
        Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    private static string CreateVerificationCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string HashVerificationCode(string email, string code)
    {
        var payload = $"{NormalizeEmail(email)}:{code.Trim()}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }

    private int GetCodeExpirationMinutes() =>
        _emailVerificationOptions.CodeExpirationMinutes > 0
            ? _emailVerificationOptions.CodeExpirationMinutes
            : 15;

    private string? GetExposedCode(string? code) =>
        _emailVerificationOptions.ExposeCodeInResponse ? code : null;
}
