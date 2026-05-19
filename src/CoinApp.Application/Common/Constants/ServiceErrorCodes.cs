namespace CoinApp.Application.Common.Constants;

public static class ServiceErrorCodes
{
    public const string CoinNotFound = "coin.not_found";
    public const string ValidationFailed = "validation.failed";
    public const string UnexpectedError = "common.unexpected_error";
    public const string AuthEmailAlreadyExists = "auth.email_already_exists";
    public const string AuthInvalidCredentials = "auth.invalid_credentials";
    public const string AuthUnauthenticated = "auth.unauthenticated";
    public const string AuthEmailNotVerified = "auth.email_not_verified";
    public const string AuthEmailAlreadyVerified = "auth.email_already_verified";
    public const string AuthEmailVerificationCodeInvalid = "auth.email_verification_code_invalid";
    public const string AuthEmailVerificationCodeExpired = "auth.email_verification_code_expired";
    public const string AuthReferralCodeInvalid = "auth.referral_code_invalid";
    public const string AuthPasswordResetCodeInvalid = "auth.password_reset_code_invalid";
    public const string AuthPasswordResetCodeExpired = "auth.password_reset_code_expired";
    public const string AuthPasswordResetTokenInvalid = "auth.password_reset_token_invalid";
    public const string AuthPasswordResetTokenExpired = "auth.password_reset_token_expired";
    public const string UserNotFound = "auth.user_not_found";
    public const string UserInactive = "auth.user_inactive";
    public const string ProfileInvalidVerificationData = "profile.invalid_verification_data";
    public const string ProfileInvalidGender = "profile.invalid_gender";
    public const string ProfileIdentityDocumentRequired = "profile.identity_document_required";
    public const string ProfileIdentityDocumentInvalid = "profile.identity_document_invalid";
    public const string AccountNotFound = "account.not_found";
    public const string AccountInvalidType = "account.invalid_type";
    public const string WalletInvalidAmount = "wallet.invalid_amount";
    public const string WalletInsufficientBalance = "wallet.insufficient_balance";
    public const string WalletBankAccountRequired = "wallet.bank_account_required";
    public const string SupportTicketNotFound = "ticket.not_found";
    public const string SupportTicketInvalidType = "ticket.invalid_type";
    public const string SupportTicketInvalidPriority = "ticket.invalid_priority";
    public const string SupportTicketInvalidStatus = "ticket.invalid_status";
    public const string SupportTicketClosed = "ticket.closed";
}
