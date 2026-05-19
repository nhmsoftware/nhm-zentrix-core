using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Auth;

namespace CoinApp.Application.Services.Auth;

public interface IAuthService
{
    Task<ServiceResult<RegisterResponseDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<EmailVerificationResponseDto>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<EmailVerificationResponseDto>> ResendEmailVerificationAsync(ResendEmailVerificationRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<AuthUserDto>> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<LogoutResponseDto>> LogoutAsync(CancellationToken cancellationToken = default);
}
