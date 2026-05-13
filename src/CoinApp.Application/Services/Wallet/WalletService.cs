using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Wallet;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;

namespace CoinApp.Application.Services.Wallet;

public sealed class WalletService : IWalletService
{
    private const int PageSize = 10;

    private readonly IUserRepository _userRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public WalletService(
        IUserRepository userRepository,
        IWalletTransactionRepository walletTransactionRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<PaginatedResult<WalletTransactionDto>>> GetCurrentUserTransactionsAsync(int page, CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<PaginatedResult<WalletTransactionDto>>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        var query = _walletTransactionRepository
            .QueryByUser(_currentUserContext.UserId.Value)
            .OrderByDescending(x => x.CreatedAtUtc);

        var result = await _walletTransactionRepository.PaginateAsync(query, page, PageSize, cancellationToken);
        var items = result.Items.Select(MapTransaction).ToList();

        return ServiceResult<PaginatedResult<WalletTransactionDto>>.Success(new PaginatedResult<WalletTransactionDto>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount));
    }

    public async Task<ServiceResult<WalletTransactionDto>> WithdrawAsync(WithdrawWalletRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Money <= 0)
        {
            return ServiceResult<WalletTransactionDto>.Failure(ServiceErrorCodes.WalletInvalidAmount, ServiceErrorCodes.WalletInvalidAmount);
        }

        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<WalletTransactionDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        var user = await _userRepository.GetByIdAsync(_currentUserContext.UserId.Value, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return ServiceResult<WalletTransactionDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        if (string.IsNullOrWhiteSpace(user.BinBank) ||
            string.IsNullOrWhiteSpace(user.AccountBank) ||
            string.IsNullOrWhiteSpace(user.AccountBankName))
        {
            return ServiceResult<WalletTransactionDto>.Failure(ServiceErrorCodes.WalletBankAccountRequired, ServiceErrorCodes.WalletBankAccountRequired);
        }

        if (request.Money > user.MoneyBalance)
        {
            return ServiceResult<WalletTransactionDto>.Failure(ServiceErrorCodes.WalletInsufficientBalance, ServiceErrorCodes.WalletInsufficientBalance);
        }

        var balanceBefore = user.MoneyBalance;
        user.MoneyBalance -= request.Money;

        var transaction = new WalletTransaction
        {
            UserId = user.Id,
            Type = WalletTransactionType.Withdraw,
            Status = WalletTransactionStatus.Pending,
            Money = request.Money,
            BalanceBefore = balanceBefore,
            BalanceAfter = user.MoneyBalance,
            Note = "wallet.withdraw_requested"
        };

        _userRepository.Update(user);
        await _walletTransactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<WalletTransactionDto>.Success(MapTransaction(transaction));
    }

    private static WalletTransactionDto MapTransaction(WalletTransaction transaction) =>
        new(
            transaction.Id,
            transaction.Type.ToString().ToLowerInvariant(),
            (int)transaction.Type,
            transaction.Status.ToString().ToLowerInvariant(),
            (int)transaction.Status,
            transaction.Money,
            transaction.BalanceBefore,
            transaction.BalanceAfter,
            transaction.Note,
            transaction.CreatedAtUtc);
}
