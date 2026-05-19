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
    private readonly IPasswordResetCodeRepository _passwordResetCodeRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IAccessTokenService _accessTokenService;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly EmailVerificationOptions _emailVerificationOptions;
    private readonly PasswordResetOptions _passwordResetOptions;

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationCodeRepository emailVerificationCodeRepository,
        IPasswordResetCodeRepository passwordResetCodeRepository,
        IPasswordHashService passwordHashService,
        IAccessTokenService accessTokenService,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUserContext,
        EmailVerificationOptions emailVerificationOptions,
        PasswordResetOptions passwordResetOptions)
    {
        _userRepository = userRepository;
        _emailVerificationCodeRepository = emailVerificationCodeRepository;
        _passwordResetCodeRepository = passwordResetCodeRepository;
        _passwordHashService = passwordHashService;
        _accessTokenService = accessTokenService;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
        _currentUserContext = currentUserContext;
        _emailVerificationOptions = emailVerificationOptions;
        _passwordResetOptions = passwordResetOptions;
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

    public async Task<ServiceResult<ForgotPasswordResponseDto>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ServiceResult<ForgotPasswordResponseDto>.Success(
                new ForgotPasswordResponseDto(normalizedEmail, null, null),
                ServiceMessageCodes.AuthPasswordResetCodeSent);
        }

        var now = DateTime.UtcNow;
        var pendingCodes = await _passwordResetCodeRepository.GetPendingByEmailAsync(normalizedEmail, cancellationToken);

        foreach (var pendingCode in pendingCodes)
        {
            pendingCode.ConsumedAtUtc = now;
            _passwordResetCodeRepository.Update(pendingCode);
        }

        var resetCode = CreateVerificationCode();
        var expiresAtUtc = now.AddMinutes(GetPasswordResetCodeExpirationMinutes());
        var passwordResetCode = new PasswordResetCode
        {
            UserId = user.Id,
            Email = user.Email,
            CodeHash = HashVerificationCode(user.Email, resetCode),
            ExpiresAtUtc = expiresAtUtc
        };

        await _passwordResetCodeRepository.AddAsync(passwordResetCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _emailSender.SendPasswordResetCodeAsync(user.Email, user.FullName, resetCode, expiresAtUtc, cancellationToken);

        return ServiceResult<ForgotPasswordResponseDto>.Success(
            new ForgotPasswordResponseDto(user.Email, expiresAtUtc, GetExposedPasswordResetCode(resetCode)),
            ServiceMessageCodes.AuthPasswordResetCodeSent);
    }

    public async Task<ServiceResult<VerifyResetCodeResponseDto>> VerifyResetCodeAsync(VerifyResetCodeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ServiceResult<VerifyResetCodeResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetCodeInvalid,
                ServiceErrorCodes.AuthPasswordResetCodeInvalid);
        }

        var resetCode = await _passwordResetCodeRepository.GetLatestPendingByEmailAsync(normalizedEmail, cancellationToken);

        if (resetCode is null || resetCode.VerifiedAtUtc is not null)
        {
            return ServiceResult<VerifyResetCodeResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetCodeInvalid,
                ServiceErrorCodes.AuthPasswordResetCodeInvalid);
        }

        var now = DateTime.UtcNow;

        if (resetCode.ExpiresAtUtc < now)
        {
            resetCode.ConsumedAtUtc = now;
            _passwordResetCodeRepository.Update(resetCode);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<VerifyResetCodeResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetCodeExpired,
                ServiceErrorCodes.AuthPasswordResetCodeExpired);
        }

        var codeHash = HashVerificationCode(normalizedEmail, request.Code);

        if (!string.Equals(resetCode.CodeHash, codeHash, StringComparison.Ordinal))
        {
            resetCode.AttemptCount++;

            if (resetCode.AttemptCount >= GetPasswordResetMaxVerifyAttempts())
            {
                resetCode.ConsumedAtUtc = now;
            }

            _passwordResetCodeRepository.Update(resetCode);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<VerifyResetCodeResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetCodeInvalid,
                ServiceErrorCodes.AuthPasswordResetCodeInvalid);
        }

        var resetToken = CreateResetToken();
        var resetTokenExpiresAtUtc = now.AddMinutes(GetPasswordResetTokenExpirationMinutes());
        resetCode.VerifiedAtUtc = now;
        resetCode.ResetTokenHash = HashResetToken(resetToken);
        resetCode.ResetTokenExpiresAtUtc = resetTokenExpiresAtUtc;

        _passwordResetCodeRepository.Update(resetCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<VerifyResetCodeResponseDto>.Success(
            new VerifyResetCodeResponseDto(user.Email, resetToken, resetTokenExpiresAtUtc),
            ServiceMessageCodes.AuthPasswordResetCodeVerified);
    }

    public async Task<ServiceResult<ResetPasswordResponseDto>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var resetTokenHash = HashResetToken(request.ResetToken);
        var resetCode = await _passwordResetCodeRepository.GetByResetTokenHashAsync(resetTokenHash, cancellationToken);

        if (resetCode is null || resetCode.VerifiedAtUtc is null || resetCode.ResetTokenExpiresAtUtc is null)
        {
            return ServiceResult<ResetPasswordResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetTokenInvalid,
                ServiceErrorCodes.AuthPasswordResetTokenInvalid);
        }

        var now = DateTime.UtcNow;

        if (resetCode.ResetTokenExpiresAtUtc < now)
        {
            resetCode.ConsumedAtUtc = now;
            _passwordResetCodeRepository.Update(resetCode);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ServiceResult<ResetPasswordResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetTokenExpired,
                ServiceErrorCodes.AuthPasswordResetTokenExpired);
        }

        var user = await _userRepository.GetByIdAsync(resetCode.UserId, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ServiceResult<ResetPasswordResponseDto>.Failure(
                ServiceErrorCodes.AuthPasswordResetTokenInvalid,
                ServiceErrorCodes.AuthPasswordResetTokenInvalid);
        }

        user.PasswordHash = _passwordHashService.HashPassword(user, request.Password);
        resetCode.ConsumedAtUtc = now;

        _userRepository.Update(user);
        _passwordResetCodeRepository.Update(resetCode);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _accessTokenService.RevokeUserTokensAsync(user.Id, cancellationToken);

        return ServiceResult<ResetPasswordResponseDto>.Success(
            new ResetPasswordResponseDto(true),
            ServiceMessageCodes.AuthPasswordResetCompleted);
    }

    public async Task<ServiceResult<ChangePasswordResponseDto>> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<ChangePasswordResponseDto>.Failure(
                ServiceErrorCodes.AuthUnauthenticated,
                ServiceErrorCodes.AuthUnauthenticated);
        }

        var user = await _userRepository.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (user is null)
        {
            return ServiceResult<ChangePasswordResponseDto>.Failure(
                ServiceErrorCodes.UserNotFound,
                ServiceErrorCodes.UserNotFound);
        }

        if (!user.IsActive)
        {
            return ServiceResult<ChangePasswordResponseDto>.Failure(
                ServiceErrorCodes.UserInactive,
                ServiceErrorCodes.UserInactive);
        }

        var isCurrentPasswordValid = _passwordHashService.VerifyPassword(user, request.CurrentPassword, user.PasswordHash);

        if (!isCurrentPasswordValid)
        {
            return ServiceResult<ChangePasswordResponseDto>.Failure(
                ServiceErrorCodes.AuthCurrentPasswordInvalid,
                ServiceErrorCodes.AuthCurrentPasswordInvalid);
        }

        user.PasswordHash = _passwordHashService.HashPassword(user, request.NewPassword);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _accessTokenService.RevokeUserTokensAsync(user.Id, cancellationToken);

        return ServiceResult<ChangePasswordResponseDto>.Success(
            new ChangePasswordResponseDto(true),
            ServiceMessageCodes.AuthPasswordChanged);
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

    private static string CreateResetToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string HashResetToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()));
        return Convert.ToHexString(bytes);
    }

    private int GetCodeExpirationMinutes() =>
        _emailVerificationOptions.CodeExpirationMinutes > 0
            ? _emailVerificationOptions.CodeExpirationMinutes
            : 15;

    private int GetPasswordResetCodeExpirationMinutes() =>
        _passwordResetOptions.CodeExpirationMinutes > 0
            ? _passwordResetOptions.CodeExpirationMinutes
            : 15;

    private int GetPasswordResetTokenExpirationMinutes() =>
        _passwordResetOptions.ResetTokenExpirationMinutes > 0
            ? _passwordResetOptions.ResetTokenExpirationMinutes
            : 15;

    private int GetPasswordResetMaxVerifyAttempts() =>
        _passwordResetOptions.MaxVerifyAttempts > 0
            ? _passwordResetOptions.MaxVerifyAttempts
            : 5;

    private string? GetExposedCode(string? code) =>
        _emailVerificationOptions.ExposeCodeInResponse ? code : null;

    private string? GetExposedPasswordResetCode(string? code) =>
        _passwordResetOptions.ExposeCodeInResponse ? code : null;
}
