using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Common;

namespace CoinApp.Application.Services.Common;

public interface ICommonLookupService
{
    Task<ServiceResult<IReadOnlyList<BankDto>>> GetBanksAsync(CancellationToken cancellationToken = default);
}
