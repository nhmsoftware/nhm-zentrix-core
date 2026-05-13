using CoinApp.Application.Common.Results;
using CoinApp.Application.Dtos.Common;
using CoinApp.Application.Interfaces.Repositories;

namespace CoinApp.Application.Services.Common;

public sealed class CommonLookupService : ICommonLookupService
{
    private readonly IBankRepository _bankRepository;

    public CommonLookupService(IBankRepository bankRepository)
    {
        _bankRepository = bankRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<BankDto>>> GetBanksAsync(CancellationToken cancellationToken = default)
    {
        var banks = await _bankRepository.GetActiveAsync(cancellationToken);
        var result = banks
            .OrderBy(x => x.ShortName)
            .Select(x => new BankDto(x.Id, x.Bin, x.Code, x.Name, x.ShortName))
            .ToList();

        return ServiceResult<IReadOnlyList<BankDto>>.Success(result);
    }
}
