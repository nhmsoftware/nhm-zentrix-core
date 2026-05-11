using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CoinApp.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<WebApplicationFactory<global::Program>>
{
    private readonly WebApplicationFactory<global::Program> _factory;

    public ApiSmokeTests(WebApplicationFactory<global::Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Factory_CanBeCreated()
    {
        Assert.NotNull(_factory);
    }
}
