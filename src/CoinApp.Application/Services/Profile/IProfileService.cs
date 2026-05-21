using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Profile;

namespace CoinApp.Application.Services.Profile;

public interface IProfileService
{
    Task<ServiceResult<UserProfileDto>> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<UserProfileDto>> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<UserProfileDto>> SubmitAccountVerificationAsync(SubmitAccountVerificationRequest request, CancellationToken cancellationToken = default);
}
