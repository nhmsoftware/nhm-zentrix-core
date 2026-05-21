namespace CoinApp.Application.Common.Constants;

public static class ServiceMessageCodes
{
    public const string CommonSuccess = "common.success";
    public const string AuthRegistered = "auth.registered";
    public const string AuthEmailVerified = "auth.email_verified";
    public const string AuthEmailAlreadyVerified = "auth.email_already_verified";
    public const string AuthEmailVerificationCodeSent = "auth.email_verification_code_sent";
    public const string AuthPasswordResetCodeSent = "auth.password_reset_code_sent";
    public const string AuthPasswordResetCodeVerified = "auth.password_reset_code_verified";
    public const string AuthPasswordResetCompleted = "auth.password_reset_completed";
    public const string AuthPasswordChanged = "auth.password_changed";
    public const string ProfileUpdated = "profile.updated";
}
