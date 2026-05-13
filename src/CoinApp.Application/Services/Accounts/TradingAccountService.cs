using CoinApp.Application.Common.Constants;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Accounts;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Domain.Entities;
using CoinApp.Domain.Enums;

namespace CoinApp.Application.Services.Accounts;

public sealed class TradingAccountService : ITradingAccountService
{
    private const int PageSize = 10;

    private readonly ITradingAccountRepository _tradingAccountRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IUnitOfWork _unitOfWork;

    public TradingAccountService(
        ITradingAccountRepository tradingAccountRepository,
        ICurrentUserContext currentUserContext,
        IUnitOfWork unitOfWork)
    {
        _tradingAccountRepository = tradingAccountRepository;
        _currentUserContext = currentUserContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<PaginatedResult<TradingAccountDto>>> GetCurrentUserAccountsAsync(string? type, int page, CancellationToken cancellationToken = default)
    {
        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<PaginatedResult<TradingAccountDto>>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        if (!TryParseType(type, out var accountType))
        {
            return ServiceResult<PaginatedResult<TradingAccountDto>>.Failure(ServiceErrorCodes.AccountInvalidType, ServiceErrorCodes.AccountInvalidType);
        }

        var query = _tradingAccountRepository
            .QueryByUser(_currentUserContext.UserId.Value, accountType)
            .OrderByDescending(x => x.CreatedAtUtc);

        var result = await _tradingAccountRepository.PaginateAsync(query, page, PageSize, cancellationToken);
        var items = result.Items.Select(MapAccount).ToList();

        return ServiceResult<PaginatedResult<TradingAccountDto>>.Success(new PaginatedResult<TradingAccountDto>(
            items,
            result.Page,
            result.PageSize,
            result.TotalCount));
    }

    public async Task<ServiceResult<TradingAccountDto>> ToggleProtectAccountAsync(ToggleProtectAccountRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUserContext.UserId.HasValue)
        {
            return ServiceResult<TradingAccountDto>.Failure(ServiceErrorCodes.UserNotFound, ServiceErrorCodes.UserNotFound);
        }

        var account = await _tradingAccountRepository.GetByIdForUserAsync(request.AccountId, _currentUserContext.UserId.Value, cancellationToken);

        if (account is null)
        {
            return ServiceResult<TradingAccountDto>.Failure(ServiceErrorCodes.AccountNotFound, ServiceErrorCodes.AccountNotFound);
        }

        account.ActiveProtectCost = !account.ActiveProtectCost;
        _tradingAccountRepository.Update(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ServiceResult<TradingAccountDto>.Success(MapAccount(account));
    }

    private static bool TryParseType(string? value, out TradingAccountType? accountType)
    {
        accountType = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        accountType = value.Trim().ToLowerInvariant() switch
        {
            "demo" => TradingAccountType.Demo,
            "real" => TradingAccountType.Real,
            _ => null
        };

        return accountType.HasValue;
    }

    private static TradingAccountDto MapAccount(TradingAccount account) =>
        new(
            account.Id,
            account.Type.ToString().ToLowerInvariant(),
            (int)account.Type,
            account.AccountNumber,
            account.Name,
            account.Balance,
            account.ActiveProtectCost);
}
