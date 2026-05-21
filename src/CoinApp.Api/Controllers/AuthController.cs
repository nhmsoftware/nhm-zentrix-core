using CoinApp.Api.Contracts.Requests.Profile;
using CoinApp.Api.Localization;
using CoinApp.Application.Common.Storage;
using CoinApp.Application.Dtos.Auth;
using CoinApp.Application.Dtos.Profile;
using CoinApp.Application.Services.Auth;
using CoinApp.Application.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinApp.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IProfileService _profileService;

    public AuthController(
        IAuthService authService,
        IProfileService profileService,
        ILocalizedMessageService localizedMessageService)
        : base(localizedMessageService)
    {
        _authService = authService;
        _profileService = profileService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyEmailAsync(request, cancellationToken);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("resend-verification-email")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendEmailVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResendEmailVerificationAsync(request, cancellationToken);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyResetCodeAsync(request, cancellationToken);
        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _authService.GetCurrentUserAsync(cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpGet("user-profile")]
    public async Task<IActionResult> GetUserProfile(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetCurrentUserProfileAsync(cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpPut("user-profile")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _profileService.UpdateProfileAsync(request, cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.ChangePasswordAsync(request, cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpPost("verify-account")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> VerifyAccount([FromForm] VerifyAccountFormRequest request, CancellationToken cancellationToken)
    {
        using var frontStream = request.CccdFrontImage?.OpenReadStream();
        using var backStream = request.CccdBackImage?.OpenReadStream();

        var applicationRequest = new SubmitAccountVerificationRequest
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.Dob,
            Gender = request.Gender,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            BinBank = request.BinBank,
            AccountBank = request.AccountBank,
            AccountBankName = request.AccountBankName,
            CccdFrontImage = CreateFileUpload(request.CccdFrontImage, frontStream),
            CccdBackImage = CreateFileUpload(request.CccdBackImage, backStream)
        };

        var result = await _profileService.SubmitAccountVerificationAsync(applicationRequest, cancellationToken);
        return FromResult(result);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(cancellationToken);
        return FromResult(result);
    }

    private static FileUpload? CreateFileUpload(IFormFile? file, Stream? stream)
    {
        return file is null || stream is null
            ? null
            : new FileUpload(file.FileName, file.ContentType, file.Length, stream);
    }
}
