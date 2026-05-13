using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Accounts;

namespace CoinApp.Application.Services.Accounts;

public interface ITradingAccountService
{
    Task<ServiceResult<PaginatedResult<TradingAccountDto>>> GetCurrentUserAccountsAsync(string? type, int page, CancellationToken cancellationToken = default);
    Task<ServiceResult<TradingAccountDto>> ToggleProtectAccountAsync(ToggleProtectAccountRequest request, CancellationToken cancellationToken = default);
}
