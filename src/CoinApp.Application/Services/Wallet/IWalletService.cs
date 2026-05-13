using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Wallet;

namespace CoinApp.Application.Services.Wallet;

public interface IWalletService
{
    Task<ServiceResult<PaginatedResult<WalletTransactionDto>>> GetCurrentUserTransactionsAsync(int page, CancellationToken cancellationToken = default);
    Task<ServiceResult<WalletTransactionDto>> WithdrawAsync(WithdrawWalletRequest request, CancellationToken cancellationToken = default);
}
