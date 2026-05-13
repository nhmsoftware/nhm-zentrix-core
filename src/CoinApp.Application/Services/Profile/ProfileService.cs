using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Common.Storage;
using CoinApp.Application.Dtos.Profile;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;

namespace CoinApp.Application.Services.Profile;

public sealed class ProfileService : IProfileService
{
    private const long MaxIdentityDocumentBytes = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedIdentityDocumentContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public ProfileService(
        IUserRepository userRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _userRepository = userRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<ServiceResult<UserProfileDto>> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentActiveUserAsync(cancellationToken);

        if (user is null)
        {
            return ServiceResult<UserProfileDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        return ServiceResult<UserProfileDto>.Success(MapUserProfile(user));
    }

    public async Task<ServiceResult<UserProfileDto>> SubmitAccountVerificationAsync(SubmitAccountVerificationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await GetCurrentActiveUserAsync(cancellationToken);

        if (user is null)
        {
            return ServiceResult<UserProfileDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        if (!TryParseGender(request.Gender, out var gender))
        {
            return ServiceResult<UserProfileDto>.Failure(ServiceErrorCodes.ProfileInvalidGender, ServiceErrorCodes.ProfileInvalidGender);
        }

        if (!HasRequiredVerificationData(request))
        {
            return ServiceResult<UserProfileDto>.Failure(
                ServiceErrorCodes.ProfileInvalidVerificationData,
                ServiceErrorCodes.ProfileInvalidVerificationData);
        }

        if (request.CccdFrontImage is null || request.CccdBackImage is null)
        {
            return ServiceResult<UserProfileDto>.Failure(
                ServiceErrorCodes.ProfileIdentityDocumentRequired,
                ServiceErrorCodes.ProfileIdentityDocumentRequired);
        }

        if (!IsValidIdentityDocument(request.CccdFrontImage) || !IsValidIdentityDocument(request.CccdBackImage))
        {
            return ServiceResult<UserProfileDto>.Failure(
                ServiceErrorCodes.ProfileIdentityDocumentInvalid,
                ServiceErrorCodes.ProfileIdentityDocumentInvalid);
        }

        var folder = $"identity/{user.Id:N}";
        var frontImage = await _fileStorageService.SaveAsync(folder, request.CccdFrontImage!, cancellationToken);
        var backImage = await _fileStorageService.SaveAsync(folder, request.CccdBackImage!, cancellationToken);

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.FullName = $"{user.FirstName} {user.LastName}".Trim();
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = gender;
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.Address = request.Address.Trim();
        user.BinBank = request.BinBank.Trim();
        user.AccountBank = request.AccountBank.Trim();
        user.AccountBankName = request.AccountBankName.Trim();
        user.IdentityFrontImagePath = frontImage.Path;
        user.IdentityBackImagePath = backImage.Path;
        user.VerificationStatus = AccountVerificationStatus.Waiting;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<UserProfileDto>.Success(MapUserProfile(user));
    }

    private async Task<User?> GetCurrentActiveUserAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || !_currentUserContext.UserId.HasValue)
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);
        return user is { IsActive: true } ? user : null;
    }

    private static bool IsValidIdentityDocument(FileUpload? file)
    {
        return file is not null &&
               file.Length > 0 &&
               file.Length <= MaxIdentityDocumentBytes &&
               AllowedIdentityDocumentContentTypes.Contains(file.ContentType);
    }

    private static bool HasRequiredVerificationData(SubmitAccountVerificationRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.FirstName) &&
               !string.IsNullOrWhiteSpace(request.LastName) &&
               request.DateOfBirth != default &&
               !string.IsNullOrWhiteSpace(request.PhoneNumber) &&
               !string.IsNullOrWhiteSpace(request.Address) &&
               !string.IsNullOrWhiteSpace(request.BinBank) &&
               !string.IsNullOrWhiteSpace(request.AccountBank) &&
               !string.IsNullOrWhiteSpace(request.AccountBankName);
    }

    private static bool TryParseGender(string value, out UserGender gender)
    {
        gender = value.Trim().ToLowerInvariant() switch
        {
            "male" => UserGender.Male,
            "female" => UserGender.Female,
            "other" => UserGender.Other,
            _ => UserGender.Unspecified
        };

        return gender != UserGender.Unspecified;
    }

    private static UserProfileDto MapUserProfile(User user)
    {
        var bank = string.IsNullOrWhiteSpace(user.BinBank) ||
                   string.IsNullOrWhiteSpace(user.AccountBank) ||
                   string.IsNullOrWhiteSpace(user.AccountBankName)
            ? null
            : new BankAccountDto(user.BinBank, user.AccountBank, user.AccountBankName);

        return new UserProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.Gender.ToString().ToLowerInvariant(),
            (int)user.Gender,
            user.PhoneNumber,
            user.Address,
            user.MoneyBalance,
            user.VerificationStatus.ToString().ToLowerInvariant(),
            (int)user.VerificationStatus,
            user.ReferralCode,
            bank,
            user.IsActive);
    }
}
