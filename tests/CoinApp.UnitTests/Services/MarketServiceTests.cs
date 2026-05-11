using CoinApp.Application.Common.Constants;
using CoinApp.Application.Dtos.Market;
using CoinApp.Application.Interfaces.Repositories;
using CoinApp.Application.Services.Market;
using CoinApp.Domain.Entities;
using Xunit;

namespace CoinApp.UnitTests.Services;

public sealed class MarketServiceTests
{
    [Fact]
    public async Task GetCoinBySymbolAsync_ReturnsFailure_WhenCoinMissing()
    {
        var service = new MarketService(new FakeCoinRepository());

        var result = await service.GetCoinBySymbolAsync(new GetCoinBySymbolRequest
        {
            Symbol = "BTC"
        });

        Assert.False(result.Succeeded);
        Assert.Equal(ServiceErrorCodes.CoinNotFound, result.ErrorCode);
    }

    private sealed class FakeCoinRepository : ICoinRepository
    {
        public IQueryable<Coin> Query() => Array.Empty<Coin>().AsQueryable();

        public Task<Coin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Coin?>(null);

        public Task AddAsync(Coin entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Coin entity)
        {
        }

        public void Delete(Coin entity)
        {
        }

        public Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<Coin, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<Coin?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default) =>
            Task.FromResult<Coin?>(null);

        public Task<IReadOnlyList<Coin>> GetActiveAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Coin>>(Array.Empty<Coin>());

        public Task<CoinApp.Application.Common.Results.PaginatedResult<Coin>> PaginateAsync(IQueryable<Coin> query, int page, int pageSize, CancellationToken cancellationToken = default) =>
            Task.FromResult(new CoinApp.Application.Common.Results.PaginatedResult<Coin>(Array.Empty<Coin>(), 1, 10, 0));
    }
}
